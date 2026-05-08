using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WrappedToast;

/// <summary>
/// Shared base class for <see cref="ToastUIEditor"/> and <see cref="ToastUIEditorViewer"/>.
/// Handles JS module loading, options forwarding, pending markdown, style updates, and disposal.
/// </summary>
public abstract class ToastUIEditorCore : ComponentBase, IAsyncDisposable
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    /// <summary>Inline <c>style</c> attribute applied to the root <c>&lt;div&gt;</c> on first render.</summary>
    [Parameter] public string? InitialStyle { get; init; }

    /// <summary>Options bag forwarded to the TOAST UI Editor / Viewer constructor as a plain JS object.</summary>
    [Parameter] public Dictionary<string, string>? Options { get; set; }

    /// <summary>
    /// When <c>true</c> (the default) the component emits a <c>HeadContent</c> block that
    /// loads the bundled TOAST UI Editor static assets from
    /// <c>_content/WrappedToast/lib/toastui-editor/...</c>. Set to <c>false</c>
    /// when the host already loads the TOAST UI assets globally (e.g. via App.razor).
    /// </summary>
    [Parameter] public bool LoadAssets { get; set; } = true;

    protected ElementReference ElementRef;
    protected IJSObjectReference? _module;
    private string? _pendingMarkdown;

    protected virtual string JsModulePath => string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", JsModulePath);
            await _module.InvokeVoidAsync("initialize", new object?[] { ElementRef, Options });
        }
        catch (JSException)
        {
            // Initialization failed (e.g. TOAST UI runtime not yet loaded). Caller can retry by
            // setting markdown again once the page reaches a stable state.
        }

        if (_pendingMarkdown != null)
        {
            SetMarkdown(_pendingMarkdown);
            _pendingMarkdown = null;
        }
    }

    /// <summary>Set the markdown body shown by the editor or viewer. Buffers until JS init completes.</summary>
    public virtual void SetMarkdown(string markdown)
    {
        if (_module == null)
        {
            _pendingMarkdown = markdown;
            return;
        }
        _module.InvokeVoidAsync("setMarkdown", new object?[] { ElementRef, markdown });
    }

    /// <summary>Set a single inline CSS property on the root element.</summary>
    public void SetStyle(string property, string value)
    {
        var styles = new Dictionary<string, string> { { property, value } };
        SetStyle(styles);
    }

    /// <summary>Set multiple inline CSS properties on the root element in one call.</summary>
    public virtual void SetStyle(Dictionary<string, string> styles)
    {
        if (_module == null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        _module.InvokeVoidAsync("setElementStyle", new object?[] { ElementRef, styles });
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_module == null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("dispose", new object?[] { ElementRef });
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }

        GC.SuppressFinalize(this);
    }
}
