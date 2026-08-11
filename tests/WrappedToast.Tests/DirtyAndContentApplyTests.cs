using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Reflection;

namespace WrappedToast.Tests;

/// <summary>
/// Pins the content-apply guard (a parent re-render must not re-push text into the live
/// editor buffer) and the dirty flag's transitions. JS is stubbed by bUnit's loose mode,
/// so these cover the C#-side rules only — the editor change listener and the browser
/// unload guard are JS-side and verified manually.
/// </summary>
public class DirtyAndContentApplyTests : IAsyncDisposable
{
    private readonly BunitContext _ctx;

    public DirtyAndContentApplyTests()
    {
        _ctx = new BunitContext();
        _ctx.Services.AddMudServices(options => options.PopoverOptions.CheckForPopoverProvider = false);
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();

    private static object? CurrentContent(WrappedToast component)
        => typeof(WrappedToast)
            .GetField("_currentContent", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component);

    private static Task SetContentParameterAsync(IRenderedComponent<WrappedToast> cut, string content)
        => cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["Content"] = content })));

    [Fact]
    public void EmptyContent_IsStillParsed()
    {
        // Regression: the apply guard must not mistake the initial empty Content for
        // "already applied", or _currentContent stays null and SaveAsync throws.
        var cut = _ctx.Render<WrappedToast>(p => p.Add(c => c.Content, ""));

        Assert.NotNull(CurrentContent(cut.Instance));
    }

    [Fact]
    public async Task Parameter_Set_With_Unchanged_Content_Does_Not_Reapply()
    {
        var cut = _ctx.Render<WrappedToast>(p => p.Add(c => c.Content, "# hello"));
        var before = CurrentContent(cut.Instance);

        await SetContentParameterAsync(cut, "# hello");

        Assert.Same(before, CurrentContent(cut.Instance));
    }

    [Fact]
    public async Task Parameter_Set_With_Changed_Content_Reapplies()
    {
        var cut = _ctx.Render<WrappedToast>(p => p.Add(c => c.Content, "# hello"));
        var before = CurrentContent(cut.Instance);

        await SetContentParameterAsync(cut, "# changed");

        Assert.NotSame(before, CurrentContent(cut.Instance));
    }

    [Fact]
    public async Task SetContent_Force_Reapplies_Identical_Content()
    {
        // "Reload / discard my edits" must work even when canonical text is identical
        // to what was originally loaded.
        var cut = _ctx.Render<WrappedToast>(p => p.Add(c => c.Content, "# hello"));
        var before = CurrentContent(cut.Instance);

        await cut.InvokeAsync(() => cut.Instance.SetContent("# hello"));
        Assert.Same(before, CurrentContent(cut.Instance));

        await cut.InvokeAsync(() => cut.Instance.SetContent("# hello", force: true));
        Assert.NotSame(before, CurrentContent(cut.Instance));
    }

    [Fact]
    public async Task MarkDirty_Sets_IsDirty_And_Raises_One_Transition()
    {
        var transitions = new List<bool>();
        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Content, "# hello")
            .Add(c => c.OnDirtyChanged, b => transitions.Add(b)));

        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());

        Assert.True(cut.Instance.IsDirty);
        Assert.Equal([true], transitions);
    }

    [Fact]
    public async Task Content_Load_Clears_Dirty()
    {
        var cut = _ctx.Render<WrappedToast>(p => p.Add(c => c.Content, "# hello"));
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        Assert.True(cut.Instance.IsDirty);

        await SetContentParameterAsync(cut, "# reloaded");

        Assert.False(cut.Instance.IsDirty);
    }

    [Fact]
    public async Task Failed_Save_Preserves_Dirty_State_And_Edit_Mode()
    {
        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Content, "# hello")
            .Add(c => c.OnSave, EventCallback.Factory.Create<string>(this, _ => throw new InvalidOperationException("save failed"))));
        var enterEditMode = typeof(WrappedToast).GetMethod("EnterEditMode", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var saveAsync = typeof(WrappedToast).GetMethod("SaveAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        await cut.InvokeAsync(() => (Task)enterEditMode.Invoke(cut.Instance, null)!);
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        await cut.InvokeAsync(() => (Task)saveAsync.Invoke(cut.Instance, null)!);

        Assert.True(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
    }

    [Fact]
    public async Task Successful_Save_Keeps_Edit_Mode_Open()
    {
        // Contract: a successful save (manual today, automatic once autosave lands) must
        // NOT leave edit mode. Saving persists and clears dirty, but the user stays in the
        // editor. Pins the regression where SaveAsync called ExitEditMode() on success.
        // Uses a testable subclass so the buffer read does not depend on JS interop.
        var savedContent = new List<string>();
        var cut = _ctx.Render<TestableWrappedToast>(p => p
            .Add(c => c.Content, "# hello")
            .Add(c => c.OnSave, EventCallback.Factory.Create<string>(this, content =>
            {
                savedContent.Add(content);
                return Task.CompletedTask;
            })));
        cut.Instance.LiveBody = "# hello (edited)";
        var enterEditMode = typeof(WrappedToast).GetMethod("EnterEditMode", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var saveAsync = typeof(WrappedToast).GetMethod("SaveAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        await cut.InvokeAsync(() => (Task)enterEditMode.Invoke(cut.Instance, null)!);
        await cut.InvokeAsync(() => cut.Instance.MarkDirty());
        Assert.True(cut.Instance.IsEditing);
        Assert.True(cut.Instance.IsDirty);

        await cut.InvokeAsync(() => (Task)saveAsync.Invoke(cut.Instance, null)!);

        Assert.True(cut.Instance.IsEditing);
        Assert.False(cut.Instance.IsDirty);
        Assert.Single(savedContent);
        Assert.Contains("# hello (edited)", savedContent[0]);
    }

    /// <summary>
    /// Subclass that replaces the JS-backed editor buffer read with a controllable
    /// string, so save/autosave behavior can be exercised without JavaScript.
    /// </summary>
    private sealed class TestableWrappedToast : WrappedToast
    {
        public string LiveBody { get; set; } = "";
        protected override Task<string> ReadLiveBodyAsync() => Task.FromResult(LiveBody);
    }
}
