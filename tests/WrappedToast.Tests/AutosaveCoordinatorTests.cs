using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Reflection;

namespace WrappedToast.Tests;

/// <summary>
/// Product tests for the WrappedToast autosave coordinator. These protect the autosave
/// contracts that exist independently of any plan: opt-in behavior, debounced (not
/// per-keystroke) persistence, single-save serialization with the latest buffer, flush,
/// and failure handling. JS is stubbed by bUnit's loose mode; the live buffer comes from
/// <see cref="TestableWrappedToast"/>.
/// </summary>
public class AutosaveCoordinatorTests : IAsyncDisposable
{
    private readonly BunitContext _ctx;

    public AutosaveCoordinatorTests()
    {
        _ctx = new BunitContext();
        _ctx.Services.AddMudServices(options => options.PopoverOptions.CheckForPopoverProvider = false);
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();

    private static readonly MethodInfo EnterEditMode =
        typeof(WrappedToast).GetMethod("EnterEditMode", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static Task EnterEditingAsync(IRenderedComponent<TestableWrappedToast> cut)
        => cut.InvokeAsync(() => (Task)EnterEditMode.Invoke(cut.Instance, null)!);

    private static async Task<IRenderedComponent<TestableWrappedToast>> RenderAsync(
        BunitContext ctx,
        EventCallback<string> onSave,
        EventCallback<SaveStatus>? onStatusChanged = null,
        bool autosaveEnabled = true,
        TimeSpan? debounce = null)
    {
        var cut = ctx.Render<TestableWrappedToast>(p =>
        {
            p.Add(c => c.Content, "# hello");
            p.Add(c => c.OnSave, onSave);
            p.Add(c => c.AutosaveEnabled, autosaveEnabled);
            if (debounce.HasValue) p.Add(c => c.AutosaveDebounce, debounce.Value);
            if (onStatusChanged.HasValue) p.Add(c => c.OnSaveStatusChanged, onStatusChanged.Value);
        });
        await EnterEditingAsync(cut);
        return cut;
    }

    [Fact]
    public async Task Autosave_Disabled_Does_Not_Autosave()
    {
        // Autosave is opt-in: with it off, MarkDirty arms no debounce timer, so no save
        // fires even after the debounce window elapses. (FlushAsync still persists a dirty
        // buffer on demand — that is covered separately.)
        var saved = new List<string>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, c => { saved.Add(c); return Task.CompletedTask; }),
            autosaveEnabled: false,
            debounce: TimeSpan.FromMilliseconds(100));
        cut.Instance.LiveBody = "# x";

        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        await Task.Delay(500); // well past the 100ms debounce; no timer was armed
        await cut.InvokeAsync(() => { });

        Assert.Empty(saved);
        Assert.True(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Flush_Persists_Dirty_Buffer_Clears_Dirty_And_Reports_Saved()
    {
        var saved = new List<string>();
        var statuses = new List<SaveStatus>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, c => { saved.Add(c); return Task.CompletedTask; }),
            EventCallback.Factory.Create<SaveStatus>(this, s => statuses.Add(s)));
        cut.Instance.LiveBody = "# hello (edited)";

        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        Assert.True(cut.Instance.IsDirty);

        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.Single(saved);
        Assert.Contains("# hello (edited)", saved[0]);
        Assert.False(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
        Assert.Equal(SaveStatus.Saved, cut.Instance.SaveStatus);
        Assert.Contains(SaveStatus.Saved, statuses);
    }

    [Fact]
    public async Task Rapid_Changes_Then_Flush_Saves_Once_With_Latest_Buffer()
    {
        // Debounce coalescing: many changes arm a single pending save, and the persisted
        // content is whatever the buffer holds when the save actually runs.
        var saved = new List<string>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, c => { saved.Add(c); return Task.CompletedTask; }),
            debounce: TimeSpan.FromSeconds(30));

        foreach (var body in new[] { "# v1", "# v2", "# v3" })
        {
            cut.Instance.LiveBody = body;
            await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        }

        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.Single(saved);
        Assert.Contains("# v3", saved[0]);
        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Failed_Save_Reports_Failed_And_Keeps_Dirty_And_Edit_Mode()
    {
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, _ => throw new InvalidOperationException("save failed")));

        cut.Instance.LiveBody = "# dirty";
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());

        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.Equal(SaveStatus.Failed, cut.Instance.SaveStatus);
        Assert.True(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
    }

    [Fact]
    public async Task Flush_Is_Noop_When_Not_Dirty()
    {
        var saved = new List<string>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, c => { saved.Add(c); return Task.CompletedTask; }));

        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.Empty(saved);
        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Flush_Preempts_Pending_Debounce()
    {
        // A long debounce is armed, then FlushAsync persists immediately without waiting.
        var saved = new List<string>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, c => { saved.Add(c); return Task.CompletedTask; }),
            debounce: TimeSpan.FromSeconds(30));
        cut.Instance.LiveBody = "# preempted";

        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.Single(saved);
        Assert.Contains("# preempted", saved[0]);
        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Debounce_Fires_One_Save_After_Idle()
    {
        var saved = new List<string>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<string>(this, c => { saved.Add(c); return Task.CompletedTask; }),
            debounce: TimeSpan.FromMilliseconds(100));
        cut.Instance.LiveBody = "# autosaved";

        // Several changes reset the same debounce; only one save should fire after idle.
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());

        cut.WaitForAssertion(() => Assert.Single(saved), TimeSpan.FromSeconds(2));
        Assert.Contains("# autosaved", saved[0]);
        Assert.False(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
    }
}
