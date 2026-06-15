# WrappedToast API Reference

Higher-level MudBlazor component combining editor/viewer with a toolbar and optional YAML front-matter support.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Optional title rendered in the toolbar |
| `ShowTitle` | `bool` | `true` | Whether to show `Title` in the toolbar |
| `Content` | `string` | `""` | Full markdown content, optionally including `---` YAML front matter |
| `OnSave` | `EventCallback<string>` | — | Invoked on save with the full markdown including front matter |
| `InitialEditType` | `string` | `"wysiwyg"` | Initial TOAST UI editor mode (`"wysiwyg"` or `"markdown"`) |
| `ToolbarExtras` | `RenderFragment?` | `null` | Render fragment placed next to the Edit button (view mode only) |
| `ViewerLinkBaseHref` | `string?` | `null` | Base href for resolving relative markdown links in the viewer |
| `ViewerImageBaseHref` | `string?` | `null` | Base href for resolving relative markdown image sources in the viewer |

## Methods - Content Read

| Method | Return | Description |
|---|---|---|
| `SetContent(string)` | `void` | Replace the editor/viewer content programmatically |
| `GetLiveContentAsync()` | `Task<string>` | Get the live markdown body (from editor if editing, otherwise last known) |
| `GetLiveFullContentAsync()` | `Task<string>` | Get the full live content including front matter |

## Methods - Editor Manipulation

| Method | Return | Description |
|---|---|---|
| `EnsureMarkdownModeAsync()` | `Task` | Ensure editor is in markdown mode (call before positional operations) |
| `InsertTextAsync(string, int[])` | `Task` | Insert text at a position (`[lineIndex, cursorOffset]`) |
| `ReplaceSelectionAsync(string, int[], int[])` | `Task` | Replace text in a range |
| `GetSelectedTextAsync(int[], int[])` | `Task<string>` | Get text in a range |
| `GetSelectionAsync()` | `Task<int[][]>` | Get current selection as `[[line, col], [line, col]]` |
| `FindAndReplaceAsync(string, string)` | `Task<int>` | Replace all occurrences; returns count |
| `FindAndReplaceFirstAsync(string, string)` | `Task<bool>` | Replace first occurrence; returns whether replaced |
| `AppendTextAsync(string)` | `Task` | Append text to the end of the editor content |

## Properties

| Property | Type | Description |
|---|---|---|
| `IsEditing` | `bool` | Whether the component is currently in edit mode |
| `ViewerOptions` | `Dictionary<string,string>` | Options forwarded to the embedded viewer |
| `EditorOptions` | `Dictionary<string,string>` | Options forwarded to the embedded editor |
