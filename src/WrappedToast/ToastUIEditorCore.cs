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

    protected ElementReference ElementRef;
    protected IJSObjectReference? _module;
    private string? _pendingMarkdown;

    protected virtual string JsModulePath => string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _module = await JS.InvokeAsync<IJSObjectReference>("import", JsModulePath);
        await _module.InvokeVoidAsync("initialize", new object?[] { ElementRef, Options });

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

    /// <summary>Get the current HTML body from the editor or viewer.</summary>
    public virtual async Task<string> GetHtmlAsync()
    {
        if (_module is null)
        {
            return string.Empty;
        }

        return await _module.InvokeAsync<string>("getHTML", new object?[] { ElementRef });
    }

    /// <summary>Copy the current markdown body to the clipboard on the client.</summary>
    public virtual async Task CopyMarkdownToClipboardAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("copyMarkdownToClipboard", new object?[] { ElementRef });
    }

    /// <summary>Copy the current HTML body to the clipboard on the client.</summary>
    public virtual async Task CopyHtmlToClipboardAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("copyHtmlToClipboard", new object?[] { ElementRef });
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
