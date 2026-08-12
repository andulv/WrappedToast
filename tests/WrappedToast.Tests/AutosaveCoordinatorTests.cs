using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Reflection;

namespace WrappedToast.Tests;

/// <summary>
/// Product tests for the WrappedToast save coordinator: opt-in debounce behavior, serialized
/// persistence, flush, and failure handling. JS is stubbed by bUnit; the live buffer comes from
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
        EventCallback<WrappedToastSaveRequest> onSaveRequested,
        EventCallback<SaveStatus>? onStatusChanged = null,
        bool autosaveEnabled = true,
        TimeSpan? debounce = null)
    {
        var cut = ctx.Render<TestableWrappedToast>(p =>
        {
            p.Add(c => c.InitialContent, "# hello");
            p.Add(c => c.OnSaveRequested, onSaveRequested);
            p.Add(c => c.AutosaveEnabled, autosaveEnabled);
            if (debounce.HasValue) p.Add(c => c.AutosaveDebounce, debounce.Value);
            if (onStatusChanged.HasValue) p.Add(c => c.OnSaveStatusChanged, onStatusChanged.Value);
        });
        await EnterEditingAsync(cut);
        return cut;
    }

    [Fact]
    public async Task Autosave_Disabled_Does_Not_Persist_After_Debounce()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            }),
            autosaveEnabled: false,
            debounce: TimeSpan.FromMilliseconds(100));
        cut.Instance.LiveBody = "# x";

        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        await Task.Delay(500);
        await cut.InvokeAsync(() => { });

        Assert.Empty(saved);
        Assert.True(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Flush_Persists_Dirty_Buffer_And_Reports_Flush_Origin()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var statuses = new List<SaveStatus>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            }),
            EventCallback.Factory.Create<SaveStatus>(this, statuses.Add));
        cut.Instance.LiveBody = "# hello (edited)";

        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        var persisted = await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.True(persisted);
        var request = Assert.Single(saved);
        Assert.Contains("# hello (edited)", request.Content);
        Assert.Equal(WrappedToastSaveOrigin.Flush, request.Origin);
        Assert.False(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
        Assert.Equal(SaveStatus.Saved, cut.Instance.SaveStatus);
        Assert.Contains(SaveStatus.Saved, statuses);
    }

    [Fact]
    public async Task Rapid_Changes_Then_Flush_Saves_Once_With_Latest_Buffer()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            }),
            debounce: TimeSpan.FromSeconds(30));

        foreach (var body in new[] { "# v1", "# v2", "# v3" })
        {
            cut.Instance.LiveBody = body;
            await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        }

        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        var request = Assert.Single(saved);
        Assert.Contains("# v3", request.Content);
        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Failed_Save_Reports_Failed_And_Keeps_Dirty_And_Edit_Mode()
    {
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, _ => throw new InvalidOperationException("save failed")));

        cut.Instance.LiveBody = "# dirty";
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);

        var persisted = await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.False(persisted);
        Assert.Equal(SaveStatus.Failed, cut.Instance.SaveStatus);
        Assert.True(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
    }

    [Fact]
    public async Task Flush_Is_Noop_When_Not_Dirty()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            }));

        var persisted = await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        Assert.True(persisted);
        Assert.Empty(saved);
        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Flush_Preempts_Pending_Debounce()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            }),
            debounce: TimeSpan.FromSeconds(30));
        cut.Instance.LiveBody = "# preempted";

        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        await cut.InvokeAsync(() => cut.Instance.FlushAsync());

        var request = Assert.Single(saved);
        Assert.Contains("# preempted", request.Content);
        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Debounce_Fires_One_Autosave_After_Idle()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var cut = await RenderAsync(_ctx,
            EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            }),
            debounce: TimeSpan.FromMilliseconds(100));
        cut.Instance.LiveBody = "# autosaved";

        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);

        cut.WaitForAssertion(() => Assert.Single(saved), TimeSpan.FromSeconds(2));
        var request = Assert.Single(saved);
        Assert.Contains("# autosaved", request.Content);
        Assert.Equal(WrappedToastSaveOrigin.Autosave, request.Origin);
        Assert.False(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
    }
}
