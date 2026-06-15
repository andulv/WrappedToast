# ToastUIEditorViewer API Reference

Thin Blazor wrapper for the native TOAST UI Viewer. No MudBlazor dependency.

The viewer wrapper exposes direct native methods that do not require JS callback bridging. Component disposal destroys the native viewer instance.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `InitialStyle` | `string?` | `null` | Inline `style` attribute on the root `<div>` |
| `Options` | `Dictionary<string,string>?` | `null` | Options forwarded to the TOAST UI Viewer constructor |

## Methods

| Method | Return | Description |
|---|---|---|
| `SetMarkdown(string)` | `void` | Set the markdown body shown by the viewer |
| `SetMarkdownAsync(string)` | `Task` | Set the markdown body shown by the viewer |
| `DestroyAsync()` | `Task` | Destroy the native instance and release JS references |
| `IsViewerAsync()` | `Task<bool>` | Check whether the wrapped instance is a viewer |
| `IsMarkdownModeAsync()` | `Task<bool>` | Check whether the viewer is in markdown mode |
| `IsWysiwygModeAsync()` | `Task<bool>` | Check whether the viewer is in WYSIWYG mode |
| `OffAsync(string)` | `Task` | Unbind handlers for the given native event type |
