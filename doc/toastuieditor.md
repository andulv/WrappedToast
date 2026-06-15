# ToastUIEditor API Reference

Thin Blazor wrapper for the native TOAST UI Editor. No MudBlazor dependency.

The wrapper exposes native methods that can be forwarded with primitive, array, string, object, or JSON return values. Native JS callback APIs (`on`, `addHook`, `addCommand`) and DOM-node APIs (`addWidget`, `getEditorElements`) are not exposed as direct C# methods. `DestroyAsync()` and component disposal destroy the native instance.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `InitialStyle` | `string?` | `null` | Inline `style` attribute on the root `<div>` |
| `Options` | `Dictionary<string,string>?` | `null` | Options forwarded to the TOAST UI Editor constructor |

## Methods

| Method | Return | Description |
|---|---|---|
| `SetMarkdown(string, bool)` | `void` | Set the markdown body (`cursorToEnd` defaults to `true`) |
| `SetMarkdownAsync(string, bool)` | `Task` | Set the markdown body (`cursorToEnd` defaults to `true`) |
| `GetMarkdownAsync()` | `Task<string>` | Get the current markdown text |
| `SetHtmlAsync(string, bool)` | `Task` | Set the current HTML body (`cursorToEnd` defaults to `true`) |
| `GetHtmlAsync()` | `Task<string>` | Get the current HTML body |
| `BlurAsync()` | `Task` | Remove focus from the editor |
| `ChangeModeAsync(string, bool)` | `Task` | Switch mode (`"markdown"` or `"wysiwyg"`; `withoutFocus` defaults to `false`) |
| `ChangePreviewStyleAsync(string)` | `Task` | Switch markdown preview style (`"tab"` or `"vertical"`) |
| `ConvertPositionToMatchEditorModeAsync(int[], int[]?, string?)` | `Task<JsonElement>` | Convert a markdown-mode range to another editor mode |
| `ConvertPositionToMatchEditorModeAsync(int, int?, string?)` | `Task<JsonElement>` | Convert a WYSIWYG offset range to another editor mode |
| `DeleteSelectionAsync(int[], int[])` | `Task` | Delete text in a markdown-position range |
| `DeleteSelectionAsync(int, int)` | `Task` | Delete text in a WYSIWYG offset range |
| `DestroyAsync()` | `Task` | Destroy the native instance and release JS references |
| `ExecAsync(string)` | `Task` | Execute a named TOAST UI command |
| `ExecAsync(string, object)` | `Task` | Execute a named TOAST UI command with payload |
| `FocusAsync()` | `Task` | Focus the editor |
| `GetCurrentPreviewStyleAsync()` | `Task<string>` | Get the current markdown preview style |
| `GetHeightAsync()` | `Task<string>` | Get the editor height |
| `GetMinHeightAsync()` | `Task<string>` | Get the editor minimum content height |
| `GetRangeInfoOfNodeAsync()` | `Task<JsonElement>` | Get range information for the node at the current selection |
| `GetRangeInfoOfNodeAsync(int[])` | `Task<JsonElement>` | Get range information for a markdown position |
| `GetRangeInfoOfNodeAsync(int)` | `Task<JsonElement>` | Get range information for a WYSIWYG offset |
| `GetScrollTopAsync()` | `Task<double>` | Get the editor container scroll position |
| `InsertTextAsync(string)` | `Task` | Insert text at the current cursor position |
| `HideAsync()` | `Task` | Hide the editor |
| `ReplaceSelectionAsync(string, int[], int[])` | `Task` | Replace text in a range (markdown: `[line, col]` positions) |
| `ReplaceSelectionAsync(string, int, int)` | `Task` | Replace text in a WYSIWYG offset range |
| `ReplaceWithWidgetAsync(int[], int[], string)` | `Task` | Replace a markdown-position range with widget text |
| `ReplaceWithWidgetAsync(int, int, string)` | `Task` | Replace a WYSIWYG offset range with widget text |
| `GetSelectedTextAsync(int[], int[])` | `Task<string>` | Get text in a range (markdown positions) |
| `GetSelectedTextAsync(int, int)` | `Task<string>` | Get text in a WYSIWYG offset range |
| `GetSelectionAsync()` | `Task<JsonElement>` | Get current selection range |
| `SetSelectionAsync(int[], int[])` | `Task` | Set selection range (markdown positions) |
| `SetSelectionAsync(int, int)` | `Task` | Set selection range (WYSIWYG offsets) |
| `IsViewerAsync()` | `Task<bool>` | Check whether the wrapped instance is a viewer |
| `IsMarkdownModeAsync()` | `Task<bool>` | Check whether the editor is in markdown mode |
| `IsWysiwygModeAsync()` | `Task<bool>` | Check whether the editor is in WYSIWYG mode |
| `MoveCursorToEndAsync(bool)` | `Task` | Move cursor to end (`focus` defaults to `true`) |
| `MoveCursorToStartAsync(bool)` | `Task` | Move cursor to start (`focus` defaults to `true`) |
| `OffAsync(string)` | `Task` | Unbind handlers for the given native event type |
| `RemoveHookAsync(string)` | `Task` | Remove a native hook by type |
| `RemoveToolbarItemAsync(string)` | `Task` | Remove a toolbar item by native item name |
| `ResetAsync()` | `Task` | Reset the editor |
| `SetHeightAsync(string)` | `Task` | Set the editor height |
| `SetMinHeightAsync(string)` | `Task` | Set the editor minimum content height |
| `SetPlaceholderAsync(string)` | `Task` | Set the placeholder on editor surfaces |
| `SetScrollTopAsync(double)` | `Task` | Set the editor container scroll position |
| `ShowAsync()` | `Task` | Show the editor |
| `InsertToolbarItemAsync(object, object)` | `Task` | Insert a toolbar item using the native TOAST UI shapes |
