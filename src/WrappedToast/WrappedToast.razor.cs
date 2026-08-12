using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using System.Threading;

namespace WrappedToast;

/// <summary>Editor save lifecycle status surfaced to hosts for indicators.</summary>
public enum SaveStatus
{
    Idle,
    Pending,
    Saving,
    Saved,
    Failed,
}

/// <summary>
/// Combines a TOAST UI Editor and Viewer with a small MudBlazor toolbar (Edit / Save / Cancel)
/// and optional front-matter table. Consumers can inject host-specific buttons via
/// <see cref="ToolbarExtras"/> or replace the toolbar through <see cref="ToolbarOverride"/>.
/// </summary>
public partial class WrappedToast : IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<WrappedToast> Logger { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    /// <summary>Optional title rendered in the toolbar.</summary>
    [Parameter] public string Title { get; set; } = "";

    /// <summary>Whether to render <see cref="Title"/> in the toolbar.</summary>
    [Parameter] public bool ShowTitle { get; set; } = true;

    /// <summary>The full markdown content (optionally including <c>---</c> YAML-style front matter).</summary>
    [Parameter] public string Content { get; set; } = "";

    /// <summary>Invoked when the user saves edits. The argument is the full markdown including front matter.</summary>
    [Parameter] public EventCallback<string> OnSave { get; set; }

    /// <summary>
    /// Enables automatic debounced saves while editing. Off by default; hosts (e.g. FilespaceView)
    /// opt in for editable Markdown files.
    /// </summary>
    [Parameter] public bool AutosaveEnabled { get; set; }

    /// <summary>Delay after the last change before an automatic save fires.</summary>
    [Parameter] public TimeSpan AutosaveDebounce { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Maximum interval a buffer may stay unsaved under continuous editing before a forced save.</summary>
    [Parameter] public TimeSpan AutosaveMaxWait { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Raised when <see cref="SaveStatus"/> changes, so hosts can render save indicators.</summary>
    [Parameter] public EventCallback<SaveStatus> OnSaveStatusChanged { get; set; }

    /// <summary>
    /// Shows a per-editor autosave on/off toggle in the toolbar. The host opts in; the choice is
    /// persisted to localStorage and overrides <see cref="AutosaveEnabled"/> for that editor.
    /// </summary>
    [Parameter] public bool ShowAutosaveToggle { get; set; }

    /// <summary>
    /// True when the live buffer holds changes that have not been saved. Sticky: set on
    /// the first change after a load, cleared on save, on Cancel, and on content load.
    /// Undoing back to the saved text does not clear it.
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>Current save lifecycle status (see <see cref="SaveStatus"/>).</summary>
    public SaveStatus SaveStatus => _saveStatus;

    /// <summary>Raised on <see cref="IsDirty"/> transitions only.</summary>
    [Parameter] public EventCallback<bool> OnDirtyChanged { get; set; }

    /// <summary>
    /// Initial TOAST UI editor mode for the embedded editor surface.
    /// Supported values match TOAST UI Editor, such as <c>wysiwyg</c> and <c>markdown</c>.
    /// </summary>
    [Parameter] public string InitialEditType { get; set; } = "wysiwyg";

    /// <summary>
    /// Optional render fragment placed in the toolbar next to the Edit button (only shown when not editing).
    /// Use this to inject host-specific buttons such as "Print" or "Share" without coupling
    /// the package to the host's navigation.
    /// </summary>
    [Parameter] public RenderFragment? ToolbarExtras { get; set; }

    /// <summary>
    /// Optional render fragment that replaces the complete default toolbar in its existing
    /// layout slot. When set, the default title, actions, and <see cref="ToolbarExtras"/>
    /// are not rendered.
    /// </summary>
    [Parameter] public RenderFragment? ToolbarOverride { get; set; }

    /// <summary>Base href used by the viewer to resolve relative markdown links.</summary>
    [Parameter] public string? ViewerLinkBaseHref { get; set; }

    /// <summary>Base href used by the viewer to resolve relative markdown image sources.</summary>
    [Parameter] public string? ViewerImageBaseHref { get; set; }

    private ToastUIEditor _editor = null!;
    private ToastUIEditorViewer _viewer = null!;
    private FrontMatterPanel _frontMatterPanel = null!;
    private ElementReference _viewerHost;
    private IJSObjectReference? _module;
    private IJSObjectReference? _wrapper;
    private bool _isEditing;
    private bool _isSaving;
    private TextContentWithFrontMatter? _currentContent;
    private bool _currentContent_updated;
    private bool _viewerRewritePending;
    // The last Content actually applied to the editor. Null until the first apply, so
    // an initially empty document still gets parsed. Guards against re-pushing (and
    // thereby clobbering) the live buffer on every parent re-render.
    private string? _lastAppliedContent;

    // Frontmatter editing state
    private bool _isEditingFrontMatter;

    // ── Autosave coordinator ────────────────────────────────────────
    private SaveStatus _saveStatus = SaveStatus.Idle;
    private readonly SemaphoreSlim _saveGate = new(1, 1);
    private Timer? _autosaveDebounceTimer;
    private Timer? _autosaveMaxWaitTimer;
    private DateTime _autosaveDirtySinceUtc = DateTime.MinValue;
    private TaskCompletionSource? _inFlightSave;
    private bool? _autosaveOverride; // null = use AutosaveEnabled; set by the toolbar toggle and persisted.
    private bool AutosaveEffective => _autosaveOverride ?? AutosaveEnabled;

    /// <summary>Default options forwarded to <see cref="ToastUIEditorViewer"/>.</summary>
    public Dictionary<string, string> ViewerOptions { get; }

    /// <summary>Default options forwarded to <see cref="ToastUIEditor"/>.</summary>
    public Dictionary<string, string> EditorOptions { get; }


    public WrappedToast()
    {
        ViewerOptions = new Dictionary<string, string>
        {
            ["height"] = "100%",
            ["frontMatter"] = "true",
            ["viewer"] = "true",
        };
        EditorOptions = new Dictionary<string, string>
        {
            ["height"] = "100%",
            ["frontMatter"] = "true",
            ["initialEditType"] = "wysiwyg",
        };
    }

    public bool IsEditing => _isEditing;

    protected override void OnParametersSet()
    {
        EditorOptions["initialEditType"] = string.IsNullOrWhiteSpace(InitialEditType)
            ? "wysiwyg"
            : InitialEditType;

        // Only apply Content when it actually changed. Blazor calls OnParametersSet on
        // every parent render, and applying re-pushes the text into the editor — which
        // would discard whatever the user has typed since the load.
        if (!string.Equals(Content, _lastAppliedContent, StringComparison.Ordinal))
        {
            ApplyContent(Content);
        }
    }

    /// <summary>
    /// Replace the editor/viewer content programmatically. Re-applying the same string is
    /// a no-op unless <paramref name="force"/> is set; force is how a consumer discards the
    /// live buffer and restores canonical text that happens to be byte-identical to the
    /// text originally loaded.
    /// </summary>
    public void SetContent(string content, bool force = false)
    {
        if (force || !string.Equals(content, _lastAppliedContent, StringComparison.Ordinal))
        {
            ApplyContent(content);
        }

        StateHasChanged();
    }

    private void ApplyContent(string content)
    {
        _lastAppliedContent = content;
        _currentContent = TextContentWithFrontMatter.Parse(content);
        _currentContent_updated = true;
        _viewerRewritePending = true;
    }

    /// <summary>
    /// Mark the buffer dirty. Consumers call this when a save they were asked to perform
    /// was rejected, so the indicator and any unsaved-changes guards keep reflecting the
    /// retained buffer.
    /// </summary>
    public void MarkDirty()
    {
        SetDirty(true);
        if (_isEditing && AutosaveEffective)
        {
            ArmAutosave();
        }
    }

    /// <summary>
    /// The single owner of dirty state: updates the flag, repaints, toggles the browser
    /// unload guard, and notifies the consumer. Transitions only.
    /// </summary>
    private void SetDirty(bool dirty)
    {
        if (IsDirty == dirty)
        {
            return;
        }

        IsDirty = dirty;
        if (!dirty)
        {
            // Buffer matches durable storage: cancel any pending autosave and reset the
            // max-wait streak so the next edit starts a fresh window.
            _autosaveDirtySinceUtc = DateTime.MinValue;
            CancelAutosaveTimers();
        }
        StateHasChanged();
        _ = SetUnloadGuardAsync(dirty);
        _ = OnDirtyChanged.InvokeAsync(dirty);
    }

    private async Task SetUnloadGuardAsync(bool enabled)
    {
        if (_wrapper is null)
        {
            return;
        }

        try
        {
            await _wrapper.InvokeVoidAsync("setUnsavedGuard", enabled);
        }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    // ── Live-content read ──────────────────────────────────────────────

    /// <summary>
    /// Get the live markdown body from the editor (only valid while editing).
    /// When not editing, returns the last known body.
    /// </summary>
    public async Task<string> GetLiveContentAsync()
    {
        if (!_isEditing) return _currentContent?.Body ?? string.Empty;
        return await ReadLiveBodyAsync();
    }

    /// <summary>
    /// Reads the live editor body. Virtual so tests can substitute a non-JS source;
    /// production reads from the TOAST UI editor. This is the single buffer-read seam
    /// shared by manual save, autosave, and the host flush.
    /// </summary>
    protected virtual Task<string> ReadLiveBodyAsync() => _editor.GetMarkdownAsync();

    /// <summary>Get the full live content including frontmatter.</summary>
    public async Task<string> GetLiveFullContentAsync()
    {
        var body = await GetLiveContentAsync();
        if (_currentContent == null) return body;
        // Snapshot the live body so ToMarkdownWithFrontMatter is accurate
        _currentContent.Body = body;
        return _currentContent.ToMarkdownWithFrontMatter();
    }

    // ── Editor manipulation (edit mode required) ───────────────────────

    private void ThrowIfNotEditing([System.Runtime.CompilerServices.CallerMemberName] string? method = null)
    {
        if (!_isEditing)
            throw new InvalidOperationException($"{method} requires edit mode. Call EnterEditMode() first.");
    }

    /// <summary>
    /// Ensure the editor is in markdown mode. Call before positional operations
    /// so that <c>[line, col]</c> position arrays are interpreted correctly.
    /// </summary>
    public async Task EnsureMarkdownModeAsync()
    {
        ThrowIfNotEditing();
        if (!await _editor.IsMarkdownModeAsync())
        {
            await _editor.ChangeModeAsync("markdown");
        }
    }

    /// <summary>
    /// Insert text at a specific position. The editor is first switched to
    /// markdown mode so positions are <c>[lineIndex, cursorOffset]</c>.
    /// </summary>
    public async Task InsertTextAsync(string text, int[] start)
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();
        // Place cursor at start, then insert
        await _editor.SetSelectionAsync(start, start);
        await _editor.InsertTextAsync(text);
        SetDirty(true);
    }

    /// <summary>
    /// Replace text in the given range. Positions are <c>[lineIndex, cursorOffset]</c>
    /// (markdown mode is ensured automatically).
    /// </summary>
    public async Task ReplaceSelectionAsync(string text, int[] start, int[] end)
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();
        await _editor.ReplaceSelectionAsync(text, start, end);
        SetDirty(true);
    }

    /// <summary>
    /// Get text in the given range. Positions are <c>[lineIndex, cursorOffset]</c>
    /// (markdown mode is ensured automatically).
    /// </summary>
    public async Task<string> GetSelectedTextAsync(int[] start, int[] end)
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();
        return await _editor.GetSelectedTextAsync(start, end);
    }

    /// <summary>
    /// Get the current selection range as <c>[[line, col], [line, col]]</c>
    /// (markdown mode is ensured automatically).
    /// </summary>
    public async Task<int[][]> GetSelectionAsync()
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();
        return await _editor.GetMarkdownSelectionAsync();
    }

    /// <summary>
    /// Find <paramref name="find"/> in the live editor content and replace all
    /// occurrences with <paramref name="replace"/>. Returns the number of
    /// replacements made. Operates in markdown mode.
    /// </summary>
    public async Task<int> FindAndReplaceAsync(string find, string replace)
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();

        var content = await _editor.GetMarkdownAsync();
        var count = CountOccurrences(content, find);
        if (count == 0) return 0;

        var updated = content.Replace(find, replace, StringComparison.Ordinal);
        await _editor.SetMarkdownAsync(updated);
        SetDirty(true);
        return count;
    }

    /// <summary>
    /// Find <paramref name="find"/> in the live editor content and replace the
    /// first occurrence with <paramref name="replace"/>. Returns whether a
    /// replacement was made. Operates in markdown mode.
    /// </summary>
    public async Task<bool> FindAndReplaceFirstAsync(string find, string replace)
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();

        var content = await _editor.GetMarkdownAsync();
        var idx = content.IndexOf(find, StringComparison.Ordinal);
        if (idx < 0) return false;

        var updated = string.Concat(content.AsSpan(0, idx), replace, content.AsSpan(idx + find.Length));
        await _editor.SetMarkdownAsync(updated);
        SetDirty(true);
        return true;
    }

    /// <summary>
    /// Append text to the end of the editor content. Operates in markdown mode.
    /// </summary>
    public async Task AppendTextAsync(string text)
    {
        ThrowIfNotEditing();
        await EnsureMarkdownModeAsync();

        var content = await _editor.GetMarkdownAsync();
        await _editor.SetMarkdownAsync(content + text);
        SetDirty(true);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        if (string.IsNullOrEmpty(needle)) return 0;
        var count = 0;
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/WrappedToast/WrappedToast.razor.js");
            _wrapper = await _module.InvokeAsync<IJSObjectReference>("create");
            await ReadAutosavePreferenceAsync();
        }

        if (_currentContent_updated)
        {
            // A load is not an edit: suspend change reporting around the push, then clear.
            await _editor.SetChangeSuspendedAsync(true);
            try
            {
                if (_isEditing)
                {
                    await _editor.SetMarkdownAsync(_currentContent?.Body ?? string.Empty);
                }
                await _viewer.SetMarkdownAsync(_currentContent?.Body ?? string.Empty);
            }
            finally
            {
                await _editor.SetChangeSuspendedAsync(false);
            }

            SetDirty(false);
            _currentContent_updated = false;
            _viewerRewritePending = true;
        }

        if (_viewerRewritePending && _wrapper != null)
        {
            await _wrapper.InvokeVoidAsync("rewriteRelativeUrls", _viewerHost, ViewerLinkBaseHref, ViewerImageBaseHref);
            _viewerRewritePending = false;
        }
    }

    private async Task EnterEditMode()
    {
        _isEditing = true;

        // Suspended: seeding the editor with the loaded text is not an edit.
        await _editor.SetChangeSuspendedAsync(true);
        try
        {
            await _editor.SetMarkdownAsync(_currentContent?.Body ?? string.Empty);
            await _editor.SetElementStyleAsync("display", "block");
            await _viewer.SetElementStyleAsync("display", "none");
        }
        finally
        {
            await _editor.SetChangeSuspendedAsync(false);
        }

        // If frontmatter exists, enter frontmatter edit mode
        if (_currentContent?.FrontMatterRows.Count > 0)
        {
            EnterFrontMatterEditMode();
        }
    }

    private async Task ExitEditModeAsync()
    {
        _isEditing = false;
        // Leaving edit mode discards the buffer (Cancel) or follows a save; either way
        // there is nothing unsaved to guard.
        SetDirty(false);
        CancelAutosaveTimers();
        // Sync the viewer with the last persisted/loaded content before showing it. Save no
        // longer pushes content (see ExecuteSaveAsync), so the viewer is refreshed here.
        await _viewer.SetMarkdownAsync(_currentContent?.Body ?? string.Empty);
        _viewerRewritePending = true;
        await _editor.SetElementStyleAsync("display", "none");
        await _viewer.SetElementStyleAsync("display", "block");
        ExitFrontMatterEditMode();
    }

    private Task SaveAsync() => SaveAsyncInternal(exitFrontMatterEdit: true, isManual: true);

    /// <summary>
    /// Flush any pending/in-flight automatic save and persist the current buffer if dirty.
    /// Used by hosts before file switch, navigation, and agent execution. Never throws on
    /// save failure or stale rejection — the status reflects the outcome and the caller
    /// decides the fallback.
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        CancelAutosaveTimers();

        var inflight = _inFlightSave;
        if (inflight is not null)
        {
            try { await inflight.Task.WaitAsync(cancellationToken); }
            catch (OperationCanceledException) { }
        }

        if (IsDirty && _isEditing)
        {
            await SaveAsyncInternal(exitFrontMatterEdit: false, isManual: false, cancellationToken);
        }
    }

    private async Task SaveAsyncInternal(bool exitFrontMatterEdit, bool isManual, CancellationToken cancellationToken = default)
    {
        if (_currentContent == null)
        {
            throw new InvalidOperationException("No content to save");
        }

        if (isManual)
        {
            _isSaving = true;
        }
        try
        {
            await ExecuteSaveAsync(exitFrontMatterEdit, cancellationToken);
        }
        finally
        {
            if (isManual)
            {
                _isSaving = false;
            }
            StateHasChanged();
        }
    }

    /// <summary>
    /// The single serialized persistence path shared by manual Save, autosave, and flush.
    /// Acquires the save gate so only one save runs at a time; snapshots the live buffer at
    /// run time (so an earlier save can never overwrite a newer buffer); clears dirty on
    /// success; keeps the buffer + edit mode on failure.
    /// </summary>
    private async Task ExecuteSaveAsync(bool exitFrontMatterEdit, CancellationToken cancellationToken)
    {
        await _saveGate.WaitAsync(cancellationToken);
        ArgumentNullException.ThrowIfNull(_currentContent);
        var inflight = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _inFlightSave = inflight;
        try
        {
            SetSaveStatus(SaveStatus.Saving);

            _currentContent.Body = await GetLiveContentAsync();
            if (_isEditingFrontMatter)
            {
                var editedRows = _frontMatterPanel.GetEditedRows();
                _currentContent = TextContentWithFrontMatter.FromParts(editedRows, _currentContent.Body);
                if (exitFrontMatterEdit)
                {
                    ExitFrontMatterEditMode();
                }
            }

            await OnSave.InvokeAsync(_currentContent.ToMarkdownWithFrontMatter());
            SetDirty(false);
            SetSaveStatus(SaveStatus.Saved);
            // Stay in edit mode after a successful save (manual or automatic): saving must
            // not end editing. The user leaves the editor explicitly via Cancel. Save does NOT
            // re-push the editor/viewer: that moved the cursor to the end and added latency. The
            // viewer is synced on the way out of edit mode (ExitEditModeAsync).
        }
        catch (OperationCanceledException)
        {
            SetSaveStatus(SaveStatus.Failed);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Save failed; retaining the unsaved editor buffer.");
            Snackbar.Add("Couldn't save. Your edits are still available.", Severity.Error);
            SetSaveStatus(SaveStatus.Failed);
        }
        finally
        {
            _saveGate.Release();
            _inFlightSave = null;
            inflight.TrySetResult();
        }
    }

    // ── Autosave scheduling ─────────────────────────────────────────

    private void ArmAutosave()
    {
        if (AutosaveDebounce <= TimeSpan.Zero && AutosaveMaxWait <= TimeSpan.Zero)
        {
            _ = TriggerAutosaveDueAsync();
            return;
        }

        if (_autosaveDirtySinceUtc == DateTime.MinValue)
        {
            _autosaveDirtySinceUtc = DateTime.UtcNow;
        }

        if (_saveStatus != SaveStatus.Saving)
        {
            SetSaveStatus(SaveStatus.Pending);
        }

        _autosaveDebounceTimer ??= new Timer(_ => _ = TriggerAutosaveDueAsync(), null, Timeout.Infinite, Timeout.Infinite);
        _autosaveMaxWaitTimer ??= new Timer(_ => _ = TriggerAutosaveDueAsync(), null, Timeout.Infinite, Timeout.Infinite);

        if (AutosaveDebounce > TimeSpan.Zero)
        {
            _autosaveDebounceTimer.Change(AutosaveDebounce, Timeout.InfiniteTimeSpan);
        }

        if (AutosaveMaxWait > TimeSpan.Zero)
        {
            var remaining = AutosaveMaxWait - (DateTime.UtcNow - _autosaveDirtySinceUtc);
            if (remaining <= TimeSpan.Zero)
            {
                _ = TriggerAutosaveDueAsync();
            }
            else
            {
                _autosaveMaxWaitTimer.Change(remaining, Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void CancelAutosaveTimers()
    {
        _autosaveDebounceTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _autosaveMaxWaitTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async Task TriggerAutosaveDueAsync()
    {
        try
        {
            await InvokeAsync(async () =>
            {
                if (!_isEditing || !IsDirty || !AutosaveEffective)
                {
                    return;
                }

                CancelAutosaveTimers();
                await SaveAsyncInternal(exitFrontMatterEdit: false, isManual: false);
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Autosave trigger failed.");
        }
    }

    private void SetSaveStatus(SaveStatus status)
    {
        if (_saveStatus == status)
        {
            return;
        }

        _saveStatus = status;
        StateHasChanged();
        _ = OnSaveStatusChanged.InvokeAsync(status);
    }

    private const string AutosaveStorageKey = "wrappedToast.autosave";

    private async Task ReadAutosavePreferenceAsync()
    {
        if (_wrapper is null) return;
        try
        {
            var stored = await _wrapper.InvokeAsync<string>("getLocal", AutosaveStorageKey);
            if (bool.TryParse(stored, out var enabled))
            {
                _autosaveOverride = enabled;
                if (IsDirty && _isEditing && AutosaveEffective) ArmAutosave();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not read autosave preference from localStorage.");
        }
    }

    private async Task ToggleAutosaveAsync()
    {
        _autosaveOverride = !AutosaveEffective;
        try
        {
            if (_wrapper is not null)
            {
                await _wrapper.InvokeVoidAsync("setLocal", AutosaveStorageKey, _autosaveOverride.Value ? "true" : "false");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not persist autosave preference.");
        }

        if (!AutosaveEffective)
        {
            CancelAutosaveTimers();
        }
        else if (IsDirty && _isEditing)
        {
            ArmAutosave();
        }

        StateHasChanged();
    }

    private static string SaveStatusText(SaveStatus status) => status switch
    {
        SaveStatus.Pending => "Saving…",
        SaveStatus.Saving => "Saving…",
        SaveStatus.Saved => "Saved",
        SaveStatus.Failed => "Save failed",
        _ => "",
    };

    private static Color SaveStatusColor(SaveStatus status) => status switch
    {
        SaveStatus.Saved => Color.Success,
        SaveStatus.Failed => Color.Error,
        _ => Color.Info,
    };

    private async Task CopyContentToClipboardAsync()
    {
        var wrapper = _wrapper ?? throw new InvalidOperationException("WrappedToast JavaScript module is not initialized.");
        try
        {
            var instance = _isEditing ? _editor.JsInstance : _viewer.JsInstance;
            if (instance is null) throw new InvalidOperationException("Editor/viewer instance is not initialized.");
            await wrapper.InvokeVoidAsync("copyContent", instance);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to copy content to the clipboard.");
            Snackbar.Add("Could not copy content to clipboard. Check browser clipboard permissions. \n" + ex.Message, Severity.Error);
        }
    }

    private async Task CopyMarkdownToClipboardAsync()
    {
        var wrapper = _wrapper ?? throw new InvalidOperationException("WrappedToast JavaScript module is not initialized.");
        try
        {
            var instance = _isEditing ? _editor.JsInstance : _viewer.JsInstance;
            if (instance is null) throw new InvalidOperationException("Editor/viewer instance is not initialized.");
            await wrapper.InvokeVoidAsync("copyMarkdown", instance);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to copy content to the clipboard.");
            Snackbar.Add("Could not copy content to clipboard. Check browser clipboard permissions. \n" + ex.Message, Severity.Error);
        }
    }

    private async Task CopyHtmlToClipboardAsync()
    {
        var wrapper = _wrapper ?? throw new InvalidOperationException("WrappedToast JavaScript module is not initialized.");
        try
        {
            var instance = _isEditing ? _editor.JsInstance : _viewer.JsInstance;
            if (instance is null) throw new InvalidOperationException("Editor/viewer instance is not initialized.");
            await wrapper.InvokeVoidAsync("copyHtml", instance);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to copy content to the clipboard.");
            Snackbar.Add("Could not copy content to clipboard. Check browser clipboard permissions. \n" + ex.Message, Severity.Error);
        }
    }

    private async Task PrintContentAsync()
    {
        var wrapper = _wrapper ?? throw new InvalidOperationException("WrappedToast JavaScript module is not initialized.");
        try
        {
            var instance = _isEditing ? _editor.JsInstance : _viewer.JsInstance;
            if (instance is null) throw new InvalidOperationException("Editor/viewer instance is not initialized.");
            await wrapper.InvokeVoidAsync("printContent", instance, Title);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to print content.");
            Snackbar.Add("Could not print content. \n" + ex.Message, Severity.Error);
        }
    }


    // ── Frontmatter editing ────────────────────────────────────────────

    private void EnterFrontMatterEditMode()
    {
        _isEditingFrontMatter = true;
        // FrontMatterPanel clones rows internally when IsEditing becomes true.
        // Entering front-matter edit mode is not itself an edit; the panel reports actual
        // row changes through OnEdited (wired to SetDirty in the markup).
    }

    private void ExitFrontMatterEditMode()
    {
        _isEditingFrontMatter = false;
        // FrontMatterPanel clears its edit buffer when IsEditing becomes false
    }

    public async ValueTask DisposeAsync()
    {
        // Stop scheduled autosaves and dispose the coordinator timers.
        CancelAutosaveTimers();
        _autosaveDebounceTimer?.Dispose();
        _autosaveMaxWaitTimer?.Dispose();
        _autosaveDebounceTimer = null;
        _autosaveMaxWaitTimer = null;

        // Best-effort flush: an abrupt tab/page close cannot reliably complete async work,
        // but if the component is disposed while still mounted (e.g. in-app navigation) we
        // try once. Fire-and-forget; never throw from dispose.
        if (IsDirty && _isEditing && _currentContent != null)
        {
            _ = Task.Run(async () =>
            {
                try { await SaveAsyncInternal(exitFrontMatterEdit: false, isManual: false); }
                catch { /* best-effort */ }
            });
        }

        // Drop the browser unload guard before releasing the JS wrapper, or an unmount
        // while dirty would leave the prompt installed for the rest of the session.
        if (IsDirty)
        {
            await SetUnloadGuardAsync(false);
        }

        if (_module == null && _wrapper == null)
        {
            return;
        }

        var wrapper = _wrapper;
        var module = _module;
        _wrapper = null;
        _module = null;

        try
        {
            if (wrapper != null)
            {
                await wrapper.DisposeAsync();
            }

            if (module != null)
            {
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException) { }
    }
}
