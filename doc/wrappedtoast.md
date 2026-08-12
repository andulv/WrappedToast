# WrappedToast API Reference

Higher-level MudBlazor component combining editor/viewer with a toolbar and optional YAML front-matter support.

## Session model

`WrappedToast` owns the live editor session: text, dirty state, revision, autosave scheduling, and save status. `InitialContent` seeds a new component instance only. A host must not use a later parameter render as a save acknowledgement; that would reconstruct the native TOAST UI document and lose browser state.

Use `LoadExternalContent(...)` only when the host explicitly intends to discard the live buffer, such as opening another document or after the user chooses **Discard and reload**.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Optional title rendered in the toolbar |
| `ShowTitle` | `bool` | `true` | Whether to show `Title` in the toolbar |
| `InitialContent` | `string` | `""` | Full markdown used once to seed this editor session, optionally including `---` YAML front matter |
| `OnSaveRequested` | `EventCallback<WrappedToastSaveRequest>` | — | Receives an immutable content/revision/origin snapshot. Successful completion acknowledges it; throwing rejects it and retains the buffer. |
| `AutosaveEnabled` | `bool` | `false` | Enables debounced autosave |
| `AutosaveDebounce` | `TimeSpan` | 10 seconds | Delay after the last edit before autosave |
| `AutosaveMaxWait` | `TimeSpan` | 2 minutes | Maximum dirty duration during continuous edits |
| `InitialEditType` | `string` | `"wysiwyg"` | Initial TOAST UI editor mode (`"wysiwyg"` or `"markdown"`) |
| `ToolbarExtras` | `RenderFragment?` | `null` | Render fragment placed next to the Edit button (view mode only) |
| `ToolbarOverride` | `RenderFragment?` | `null` | Replaces the complete default toolbar in its existing layout slot; takes precedence over `ToolbarExtras` |
| `ViewerLinkBaseHref` | `string?` | `null` | Base href for resolving relative markdown links in the viewer |
| `ViewerImageBaseHref` | `string?` | `null` | Base href for resolving relative markdown image sources in the viewer |

## Methods - Content and persistence

| Method | Return | Description |
|---|---|---|
| `LoadExternalContent(string)` | `void` | Explicitly discard this session's buffer and load canonical content into editor/viewer |
| `GetLiveContentAsync()` | `Task<string>` | Get the live markdown body (from editor if editing, otherwise last durable content) |
| `GetLiveFullContentAsync()` | `Task<string>` | Get the full live content including front matter without changing the session |
| `FlushAsync(CancellationToken)` | `Task<bool>` | Persist the current dirty buffer now; true means the live buffer is durable. False means persistence failed, was rejected, or a newer edit remains dirty; cancellation propagates |

## Save request

`WrappedToastSaveRequest` contains:

| Property | Description |
|---|---|
| `Content` | Full markdown snapshot, including front matter |
| `Revision` | Monotonically increasing session revision for the snapshot |
| `Origin` | `Manual`, `Autosave`, or `Flush` |

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
| `IsDirty` | `bool` | Whether the live buffer has edits that are not durably acknowledged |
| `IsAutosaveEnabled` | `bool` | Effective autosave state after the local toolbar choice is applied |
| `Revision` | `long` | Monotonically increasing editor-session revision |
| `SaveStatus` | `SaveStatus` | Current persistence lifecycle status |
| `ViewerOptions` | `Dictionary<string,string>` | Options forwarded to the embedded viewer |
| `EditorOptions` | `Dictionary<string,string>` | Options forwarded to the embedded editor |
