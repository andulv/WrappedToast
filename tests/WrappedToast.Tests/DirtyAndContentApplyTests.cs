using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Reflection;

namespace WrappedToast.Tests;

/// <summary>
/// Product tests for editor-session ownership. Host parameter echoes must not replace a live
/// buffer; only an explicit external load may do that. Save acknowledgements must not mark a newer
/// revision clean.
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

    private static readonly MethodInfo EnterEditMode =
        typeof(WrappedToast).GetMethod("EnterEditMode", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo SaveAsync =
        typeof(WrappedToast).GetMethod("SaveAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static Task EnterEditingAsync(IRenderedComponent<TestableWrappedToast> cut)
        => cut.InvokeAsync(() => (Task)EnterEditMode.Invoke(cut.Instance, null)!);

    private static Task TriggerManualSaveAsync(IRenderedComponent<TestableWrappedToast> cut)
        => cut.InvokeAsync(() => (Task)SaveAsync.Invoke(cut.Instance, null)!);

    private static Task SetInitialContentParameterAsync(
        IRenderedComponent<TestableWrappedToast> cut,
        string content)
        => cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["InitialContent"] = content })));

    [Fact]
    public async Task Initial_Empty_Content_Creates_A_Usable_Session()
    {
        var cut = _ctx.Render<TestableWrappedToast>(p => p.Add(c => c.InitialContent, ""));

        Assert.Equal(string.Empty, await cut.Instance.GetLiveFullContentAsync());
    }

    [Fact]
    public async Task Later_InitialContent_Parameter_Change_Does_Not_Replace_The_Session()
    {
        var cut = _ctx.Render<TestableWrappedToast>(p => p.Add(c => c.InitialContent, "# loaded"));

        await SetInitialContentParameterAsync(cut, "# host echo");

        Assert.Equal("# loaded", await cut.Instance.GetLiveFullContentAsync());
    }

    [Fact]
    public async Task Explicit_External_Load_Replaces_The_Session_And_Clears_Dirty_State()
    {
        var cut = _ctx.Render<TestableWrappedToast>(p => p.Add(c => c.InitialContent, "# loaded"));
        await EnterEditingAsync(cut);
        cut.Instance.LiveBody = "# local edit";
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        var editedRevision = cut.Instance.Revision;

        await cut.InvokeAsync(() => cut.Instance.LoadExternalContent("# canonical reload"));

        Assert.False(cut.Instance.IsDirty);
        Assert.True(cut.Instance.Revision > editedRevision);
    }

    [Fact]
    public async Task Failed_Save_Preserves_Dirty_State_And_Edit_Mode()
    {
        var cut = _ctx.Render<TestableWrappedToast>(p => p
            .Add(c => c.InitialContent, "# hello")
            .Add(c => c.OnSaveRequested, EventCallback.Factory.Create<WrappedToastSaveRequest>(
                this,
                _ => throw new InvalidOperationException("save failed"))));
        await EnterEditingAsync(cut);
        cut.Instance.LiveBody = "# dirty";
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);

        await TriggerManualSaveAsync(cut);

        Assert.Equal(SaveStatus.Failed, cut.Instance.SaveStatus);
        Assert.True(cut.Instance.IsDirty);
        Assert.True(cut.Instance.IsEditing);
    }

    [Fact]
    public async Task Successful_Manual_Save_Stays_In_Edit_Mode_And_Uses_An_Explicit_Request()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var cut = _ctx.Render<TestableWrappedToast>(p => p
            .Add(c => c.InitialContent, "# hello")
            .Add(c => c.OnSaveRequested, EventCallback.Factory.Create<WrappedToastSaveRequest>(this, request =>
            {
                saved.Add(request);
                return Task.CompletedTask;
            })));
        cut.Instance.LiveBody = "# hello (edited)";
        await EnterEditingAsync(cut);
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);

        await TriggerManualSaveAsync(cut);

        Assert.True(cut.Instance.IsEditing);
        Assert.False(cut.Instance.IsDirty);
        var request = Assert.Single(saved);
        Assert.Contains("# hello (edited)", request.Content);
        Assert.Equal(WrappedToastSaveOrigin.Manual, request.Origin);
        Assert.Equal(cut.Instance.Revision, request.Revision);
    }

    [Fact]
    public async Task Save_Acknowledging_An_Older_Revision_Leaves_A_Newer_Edit_Dirty()
    {
        var saved = new List<WrappedToastSaveRequest>();
        var saveStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var allowSaveToFinish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cut = _ctx.Render<TestableWrappedToast>(p => p
            .Add(c => c.InitialContent, "# hello")
            .Add(c => c.OnSaveRequested, EventCallback.Factory.Create<WrappedToastSaveRequest>(this, async request =>
            {
                saved.Add(request);
                saveStarted.TrySetResult();
                await allowSaveToFinish.Task;
            })));
        await EnterEditingAsync(cut);
        cut.Instance.LiveBody = "# first";
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);

        var firstFlush = cut.InvokeAsync(() => cut.Instance.FlushAsync());
        await saveStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        cut.Instance.LiveBody = "# newer";
        await cut.InvokeAsync(cut.Instance.SimulateUserEdit);
        allowSaveToFinish.TrySetResult();
        Assert.False(await firstFlush);

        Assert.True(cut.Instance.IsDirty);
        Assert.Equal("# first", Assert.Single(saved).Content);

        Assert.True(await cut.InvokeAsync(() => cut.Instance.FlushAsync()));
        Assert.False(cut.Instance.IsDirty);
        Assert.Equal(["# first", "# newer"], saved.Select(request => request.Content));
    }
}
