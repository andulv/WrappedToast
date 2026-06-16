using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace WrappedToast;

/// <summary>
/// Shared base class for <see cref="ToastUIEditor"/> and <see cref="ToastUIEditorViewer"/>.
/// Handles JS module loading, options forwarding, pending markdown, helper interop, and disposal.
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
    protected IJSObjectReference? _instance;
    internal IJSObjectReference? JsInstance => _instance;
    private string? _pendingMarkdown;
    private bool _pendingMarkdownCursorToEnd = true;

    protected virtual string JsModulePath => string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _module = await JS.InvokeAsync<IJSObjectReference>("import", JsModulePath);
        _instance = await _module.InvokeAsync<IJSObjectReference>("initialize", new object?[] { ElementRef, Options });

        if (_pendingMarkdown != null)
        {
            await SetMarkdownCoreAsync(_pendingMarkdown, _pendingMarkdownCursorToEnd);
            _pendingMarkdown = null;
            _pendingMarkdownCursorToEnd = true;
        }
    }

    protected void SetMarkdownCore(string markdown, bool cursorToEnd = true)
    {
        _ = SetMarkdownCoreAsync(markdown, cursorToEnd);
    }

    protected async Task SetMarkdownCoreAsync(string markdown, bool cursorToEnd = true)
    {
        if (_instance == null)
        {
            _pendingMarkdown = markdown;
            _pendingMarkdownCursorToEnd = cursorToEnd;
            return;
        }
        await _instance.InvokeVoidAsync("setMarkdown", new object?[] { markdown, cursorToEnd });
    }

    protected async Task<string> InvokeStringMethodAsync(string identifier, params object?[] args)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Component not initialized");
        }
        return await _instance.InvokeAsync<string>(identifier, args);
    }

    protected async Task<string> InvokeStringStreamMethodAsync(string identifier, params object?[] args)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        var dataReference = await _instance.InvokeAsync<IJSStreamReference>(identifier, args);
        using var dataReferenceStream = await dataReference.OpenReadStreamAsync(maxAllowedSize: 10_000_000);
        using var reader = new StreamReader(dataReferenceStream);
        return await reader.ReadToEndAsync();
    }

    protected async Task<bool> InvokeBoolMethodAsync(string identifier, bool defaultValue = false, params object?[] args)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        return await _instance.InvokeAsync<bool>(identifier, args);
    }

    protected async Task<double> InvokeDoubleMethodAsync(string identifier, double defaultValue = 0, params object?[] args)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        return await _instance.InvokeAsync<double>(identifier, args);
    }

    protected async Task<JsonElement> InvokeJsonMethodAsync(string identifier, params object?[] args)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        return await _instance.InvokeAsync<JsonElement>(identifier, args);
    }

    protected async Task InvokeVoidMethodAsync(string identifier, params object?[] args)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        await _instance.InvokeVoidAsync(identifier, args);
    }

    internal void SetElementStyle(string property, string value)
    {
        SetElementStyle(new Dictionary<string, string> { { property, value } });
    }

    internal void SetElementStyle(Dictionary<string, string> styles)
    {
        if (_instance == null)
        {
            throw new InvalidOperationException("Component not initialized");
        }

        _instance.InvokeVoidAsync("setElementStyle", new object?[] { styles });
    }

    /// <summary>Destroy the native TOAST UI instance and release JS references.</summary>
    public async Task DestroyAsync()
    {
        await DisposeAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_instance == null && _module == null)
        {
            return;
        }

        var instance = _instance;
        var module = _module;
        _instance = null;
        _module = null;

        try
        {
            if (instance != null)
            {
                await instance.InvokeVoidAsync("dispose");
                await instance.DisposeAsync();
            }

            if (module != null)
            {
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException) { }

        GC.SuppressFinalize(this);
    }
}
