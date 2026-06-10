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

    private ToastUIEditor _editor = null!;
    private ToastUIEditorViewer _viewer = null!;
    private bool _isEditing;
    private bool _isSaving;
    private TextContentWithFrontMatter? _currentContent;
    private bool _currentContent_updated;

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

    protected override void OnParametersSet()
    {
        EditorOptions["initialEditType"] = string.IsNullOrWhiteSpace(InitialEditType)
            ? "wysiwyg"
            : InitialEditType;

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

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (_currentContent_updated)
        {
            if (_isEditing)
            {
                _editor.SetMarkdown(_currentContent?.Body ?? string.Empty);
            }
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
    }

    private void ExitEditMode()
    {
        _isEditing = false;
        _editor.SetStyle("display", "none");
        _viewer.SetStyle("display", "block");
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

    private static string GetFrontMatterKeyClass(int level)
        => $"frontmatter-key frontmatter-key--level-{Math.Clamp(level, 0, 4)}";
}

/// <summary>
/// Helper that splits a markdown string into optional <c>---</c> YAML-style front matter and a body,
/// and recombines them.
/// </summary>
public class TextContentWithFrontMatter
{
    public static TextContentWithFrontMatter? Parse(string fullContent)
    {
        if (fullContent == null)
        {
            return null;
        }
        var (frontMatter, body) = SplitFrontMatter(fullContent);
        var rows = ParseFrontMatterRows(frontMatter);
        return new TextContentWithFrontMatter
        {
            FrontMatterText = frontMatter,
            FrontMatterRows = rows,
            Body = body,
        };
    }

    public string? FrontMatterText { get; init; }
    public IReadOnlyList<FrontMatterRow> FrontMatterRows { get; init; } = [];
    public required string Body { get; set; }

    public string ToMarkdownWithFrontMatter()
    {
        if (string.IsNullOrWhiteSpace(FrontMatterText))
        {
            return Body;
        }

        return $"---\n{FrontMatterText.TrimEnd()}\n---\n{Body}";
    }

    private static IReadOnlyList<FrontMatterRow> ParseFrontMatterRows(string? frontMatter)
    {
        if (string.IsNullOrWhiteSpace(frontMatter))
        {
            return [];
        }

        var rows = new List<FrontMatterRow>();
        foreach (var rawLine in frontMatter.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            var trimmed = rawLine.Trim();
            if (trimmed.StartsWith('#') || trimmed == "---")
            {
                continue;
            }

            var indent = rawLine.TakeWhile(char.IsWhiteSpace).Count();
            var level = Math.Clamp(indent / 2, 0, 4);
            var separatorIndex = trimmed.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                rows.Add(new FrontMatterRow(trimmed, string.Empty, level, IsSection: false));
                continue;
            }

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();
            rows.Add(new FrontMatterRow(key, value, level, string.IsNullOrEmpty(value)));
        }

        return rows;
    }

    private static (string? frontMatter, string body) SplitFrontMatter(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return (null, content ?? string.Empty);
        }

        var firstLineEnd = content.IndexOf('\n', StringComparison.Ordinal);
        if (firstLineEnd < 0)
        {
            return (null, content);
        }

        var firstLine = content[..firstLineEnd].TrimEnd('\r');
        if (firstLine != "---")
        {
            return (null, content);
        }

        var frontMatterStart = firstLineEnd + 1;
        var lineStart = frontMatterStart;
        while (lineStart < content.Length)
        {
            var lineEnd = content.IndexOf('\n', lineStart);
            var markerLineEnd = lineEnd < 0 ? content.Length : lineEnd;
            var markerLine = content[lineStart..markerLineEnd].TrimEnd('\r');
            if (markerLine == "---")
            {
                var bodyStart = lineEnd < 0 ? content.Length : lineEnd + 1;
                var frontmatter = content[frontMatterStart..lineStart].TrimEnd('\r', '\n');
                var body = content[bodyStart..];

                return (frontmatter, body);
            }

            if (lineEnd < 0)
            {
                break;
            }

            lineStart = lineEnd + 1;
        }

        return (null, content);
    }
}

public sealed record FrontMatterRow(string Key, string Value, int Level, bool IsSection);
