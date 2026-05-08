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

    /// <summary>The full markdown content (optionally including <c>---</c> YAML-style front matter).</summary>
    [Parameter] public string Content { get; set; } = "";

    /// <summary>Invoked when the user saves edits. The argument is the full markdown including front matter.</summary>
    [Parameter] public EventCallback<string> OnSave { get; set; }

    /// <summary>
    /// Optional render fragment placed in the toolbar next to the Edit button (only shown when not editing).
    /// Use this to inject host-specific buttons such as "Print" or "Share" without coupling
    /// the package to the host's navigation.
    /// </summary>
    [Parameter] public RenderFragment? ToolbarExtras { get; set; }

    /// <summary>
    /// When <c>true</c> (default) the component asks the inner editor to render the bundled
    /// TOAST UI assets via <c>HeadContent</c>. Set to <c>false</c> if the host already
    /// loads the TOAST UI assets globally.
    /// </summary>
    [Parameter] public bool LoadAssets { get; set; } = true;

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
            ["initialEditType"] = "markdown",
        };
    }

    protected override void OnParametersSet()
    {
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
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(frontMatter))
        {
            var lines = frontMatter.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    dict[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }
        return new TextContentWithFrontMatter
        {
            FrontMatter = dict,
            Body = body,
        };
    }

    public Dictionary<string, string> FrontMatter { get; init; } = new();
    public required string Body { get; set; }

    public string ToMarkdownWithFrontMatter()
    {
        var fm = "";
        if (FrontMatter.Count > 0)
        {
            fm += "---\n";
            foreach (var kvp in FrontMatter)
            {
                fm += $"{kvp.Key}: {kvp.Value}\n";
            }
            fm += "---\n";
        }
        return fm + Body;
    }

    private static (string? frontMatter, string body) SplitFrontMatter(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return (null, content ?? string.Empty);
        }

        if (!content.StartsWith("---", StringComparison.Ordinal))
        {
            return (null, content);
        }

        var endIndex = content.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
        {
            return (null, content);
        }

        var fmEnd = endIndex + 4; // length of "\n---"
        if (fmEnd < content.Length && content[fmEnd] == '\r') fmEnd++;
        if (fmEnd < content.Length && content[fmEnd] == '\n') fmEnd++;

        var frontmatter = content[..fmEnd].TrimEnd();
        var body = content[fmEnd..];

        return (frontmatter, body);
    }
}
