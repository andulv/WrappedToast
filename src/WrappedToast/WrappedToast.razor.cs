using Microsoft.AspNetCore.Components;

namespace WrappedToast;

/// <summary>
/// Combines a TOAST UI Editor and Viewer with a small MudBlazor toolbar (Edit / Save / Cancel)
/// and optional front-matter table. Consumers can inject host-specific buttons via
/// <see cref="ToolbarExtras"/>.
/// </summary>
public partial class WrappedToast
{
    /// <summary>Optional title rendered in the toolbar.</summary>
    [Parameter] public string Title { get; set; } = "";

    /// <summary>Whether to render <see cref="Title"/> in the toolbar.</summary>
    [Parameter] public bool ShowTitle { get; set; } = true;

    /// <summary>The full markdown content (optionally including <c>---</c> YAML-style front matter).</summary>
    [Parameter] public string Content { get; set; } = "";

    /// <summary>Invoked when the user saves edits. The argument is the full markdown including front matter.</summary>
    [Parameter] public EventCallback<string> OnSave { get; set; }

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

    /// <summary>Base href used by the viewer to resolve relative markdown links.</summary>
    [Parameter] public string? ViewerLinkBaseHref { get; set; }

    /// <summary>Base href used by the viewer to resolve relative markdown image sources.</summary>
    [Parameter] public string? ViewerImageBaseHref { get; set; }

    private ToastUIEditor _editor = null!;
    private ToastUIEditorViewer _viewer = null!;
    private FrontMatterPanel _frontMatterPanel = null!;
    private bool _isEditing;
    private bool _isSaving;
    private TextContentWithFrontMatter? _currentContent;
    private bool _currentContent_updated;

    // Frontmatter editing state
    private bool _isEditingFrontMatter;

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

        if (string.IsNullOrWhiteSpace(ViewerLinkBaseHref))
        {
            ViewerOptions.Remove("linkBaseHref");
        }
        else
        {
            ViewerOptions["linkBaseHref"] = ViewerLinkBaseHref;
        }

        if (string.IsNullOrWhiteSpace(ViewerImageBaseHref))
        {
            ViewerOptions.Remove("imageBaseHref");
        }
        else
        {
            ViewerOptions["imageBaseHref"] = ViewerImageBaseHref;
        }

        _currentContent = TextContentWithFrontMatter.Parse(Content);
        _currentContent_updated = true;
    }

    /// <summary>Replace the editor/viewer content programmatically.</summary>
    public void SetContent(string content)
    {
        _currentContent = TextContentWithFrontMatter.Parse(content);
        _currentContent_updated = true;
        StateHasChanged();
    }

    // ── Live-content read ──────────────────────────────────────────────

    /// <summary>
    /// Get the live markdown body from the editor (only valid while editing).
    /// When not editing, returns the last known body.
    /// </summary>
    public async Task<string> GetLiveContentAsync()
    {
        if (!_isEditing) return _currentContent?.Body ?? string.Empty;
        return await _editor.GetMarkdownAsync();
    }

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
        return await _editor.GetSelectionAsync();
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
        _editor.SetMarkdown(updated);
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
        _editor.SetMarkdown(updated);
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
        _editor.SetMarkdown(content + text);
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

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (_currentContent_updated)
        {
            if (_isEditing)
            {
                _editor.SetMarkdown(_currentContent?.Body ?? string.Empty);
            }
            _viewer.SetLinkBaseHref(ViewerLinkBaseHref);
            _viewer.SetImageBaseHref(ViewerImageBaseHref);
            _viewer.SetMarkdown(_currentContent?.Body ?? string.Empty);
            _currentContent_updated = false;
        }
        return Task.CompletedTask;
    }

    private void EnterEditMode()
    {
        _isEditing = true;
        _editor.SetMarkdown(_currentContent?.Body ?? string.Empty);
        _editor.SetStyle("display", "block");
        _viewer.SetStyle("display", "none");

        // If frontmatter exists, enter frontmatter edit mode
        if (_currentContent?.FrontMatterRows.Count > 0)
        {
            EnterFrontMatterEditMode();
        }
    }

    private void ExitEditMode()
    {
        _isEditing = false;
        _editor.SetStyle("display", "none");
        _viewer.SetStyle("display", "block");
        ExitFrontMatterEditMode();
    }

    private async Task SaveAsync()
    {
        if (_currentContent == null)
        {
            throw new InvalidOperationException("No content to save");
        }

        _isSaving = true;
        try
        {
            _currentContent.Body = await _editor.GetMarkdownAsync();

            // If frontmatter was being edited, pull the edited rows from the panel
            if (_isEditingFrontMatter)
            {
                var editedRows = _frontMatterPanel.GetEditedRows();
                _currentContent = TextContentWithFrontMatter.FromParts(editedRows, _currentContent.Body);
                ExitFrontMatterEditMode();
            }

            await OnSave.InvokeAsync(_currentContent.ToMarkdownWithFrontMatter());
            _viewer.SetMarkdown(_currentContent.Body);
        }
        finally
        {
            _isSaving = false;
            ExitEditMode();
        }
    }

    private async Task CopyMarkdownAsync()
    {
        if (_isEditing)
        {
            await _editor.CopyMarkdownToClipboardAsync();
            return;
        }

        await _viewer.CopyMarkdownToClipboardAsync();
    }

    private async Task CopyHtmlAsync()
    {
        if (_isEditing)
        {
            await _editor.CopyHtmlToClipboardAsync();
            return;
        }

        await _viewer.CopyHtmlToClipboardAsync();
    }

    // ── Frontmatter editing ────────────────────────────────────────────

    private void EnterFrontMatterEditMode()
    {
        _isEditingFrontMatter = true;
        // FrontMatterPanel clones rows internally when IsEditing becomes true
    }

    private void ExitFrontMatterEditMode()
    {
        _isEditingFrontMatter = false;
        // FrontMatterPanel clears its edit buffer when IsEditing becomes false
    }
}
