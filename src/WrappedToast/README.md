# WrappedToast

A lightweight Blazor wrapper for [TOAST UI Editor](https://github.com/nhn/tui.editor) with integrated view/edit toolbar powered by [MudBlazor](https://mudblazor.com/).

**WrappedToast v0.1.0-preview.1** wraps [TOAST UI Editor v3.2.2](https://github.com/nhn/tui.editor/releases/tag/3.2.2).

## Overview

WrappedToast provides three Blazor components:

- **`<ToastUIEditor>`** — Rich markdown editor component
- **`<ToastUIEditorViewer>`** — Read-only markdown viewer component  
- **`<WrappedToast>`** — High-level component combining editor/viewer with a toolbar (Edit / Save / Cancel) and optional YAML front-matter display

The **TOAST UI Editor assets are bundled as static web assets** — no CDN is required. The package is optimized for NuGet distribution and standalone use outside any specific host framework.

## Installation

Add the NuGet package to your Blazor project:

```bash
dotnet add package WrappedToast
```

Or manually add to your `.csproj`:

```xml
<PackageReference Include="WrappedToast" Version="0.1.0-preview.1" />
```

### Dependencies

- `Microsoft.AspNetCore.Components.Web` (peer dependency)
- `MudBlazor` ≥ 9.4.0 (required by `<WrappedToast>`)

## Quick Start

### 1. Add imports

Add this to your `_Imports.razor`:

```razor
@using WrappedToast
```

### 2. Use the editor

```razor
<ToastUIEditor @ref="_editor"
               InitialStyle="display:block;width:100%;height:400px;"
               Options="@(new() { ["initialEditType"]="markdown" })" />

<button @onclick="ReadMarkdownAsync">Get Markdown</button>

@code {
    private ToastUIEditor _editor = null!;
    private string _markdown = "";

    private async Task ReadMarkdownAsync()
    {
        _markdown = await _editor.GetMarkdownAsync();
    }
}
```

### 3. Use the viewer

```razor
<ToastUIEditorViewer @ref="_viewer"
                     InitialStyle="display:block;width:100%;height:400px;" />

<button @onclick="@(() => _viewer.SetMarkdown(SampleMarkdown))">Load Sample</button>

@code {
    private ToastUIEditorViewer _viewer = null!;
    
    private const string SampleMarkdown = """
        # Hello World
        
        This is **bold** text and *italic* text.
        """;
}
```

### 4. Use the complete wrapper

```razor
<WrappedToast Title="My Document"
              Content="@_content"
              OnSave="SaveAsync"
              LoadAssets="false">
    <ToolbarExtras>
        <MudButton Variant="Variant.Outlined" Size="Size.Small">Print</MudButton>
    </ToolbarExtras>
</WrappedToast>

@code {
    private string _content = "---\ntitle: Sample\n---\n# Hello";

    private async Task SaveAsync(string markdown)
    {
        await SaveToDatabase(markdown);
        // Handle successful save
    }
}
```

## Features

- ✅ **Markdown editing & viewing** with TOAST UI Editor
- ✅ **YAML front-matter parsing** — automatically displays front-matter as a table
- ✅ **View/edit toolbar** — built-in Edit / Save / Cancel buttons via MudBlazor
- ✅ **Dark theme support** — bundled dark theme CSS included
- ✅ **No CDN required** — all assets bundled as static web assets
- ✅ **Extensible toolbar** — inject custom buttons via `ToolbarExtras` render fragment
- ✅ **MIT licensed** — compatible with TOAST UI Editor's license

## API Reference

### `<ToastUIEditor />`

Low-level editor component wrapping TOAST UI Editor.

#### Parameters

- `InitialStyle` (string?) — Inline CSS style applied to the root `<div>` on first render
- `Options` (Dictionary<string, string>?) — Options bag forwarded to the TOAST UI Editor constructor
- `LoadAssets` (bool) — Load bundled CSS/JS assets (default: `true`). Set to `false` if assets are loaded globally

#### Methods

- `Task<string> GetMarkdownAsync()` — Get current markdown content from the editor
- `void SetMarkdown(string markdown)` — Set markdown content
- `void SetStyle(string property, string value)` — Set a single CSS property on the root element
- `void SetStyle(Dictionary<string, string> styles)` — Set multiple CSS properties

---

### `<ToastUIEditorViewer />`

Read-only viewer component wrapping TOAST UI Editor in view mode.

#### Parameters

- `InitialStyle` (string?) — Inline CSS style applied to the root `<div>`
- `Options` (Dictionary<string, string>?) — Options forwarded to TOAST UI Editor (use `["viewer"]="true"` for view mode)
- `LoadAssets` (bool) — Load bundled CSS/JS assets (default: `true`)

#### Methods

- `void SetMarkdown(string markdown)` — Set markdown content
- `void SetStyle(string property, string value)` — Set a single CSS property
- `void SetStyle(Dictionary<string, string> styles)` — Set multiple CSS properties

---

### `<WrappedToast />`

High-level component combining editor/viewer with MudBlazor toolbar and front-matter display.

#### Parameters

- `Title` (string) — Title rendered in the toolbar
- `Content` (string) — Full markdown content (including YAML front-matter if present)
- `OnSave` (EventCallback<string>) — Invoked when user saves; passes full markdown including front-matter
- `ToolbarExtras` (RenderFragment?) — Optional render fragment for additional toolbar buttons
- `LoadAssets` (bool) — Load bundled CSS/JS (default: `true`)

#### Behavior

- **View mode (default):** Displays the markdown content as read-only via `ToastUIEditorViewer`
- **Edit mode:** Replaces viewer with `ToastUIEditor`; shows Save/Cancel buttons and disables ToolbarExtras
- **Front-matter parsing:** If the content starts with `---`, parses YAML-style front matter and displays it as a table above the content

---

## Configuration

### TOAST UI Editor Options

Pass options to the underlying TOAST UI Editor via the `Options` parameter:

```razor
<ToastUIEditor Options="@(new()
{
    ["initialEditType"] = "markdown",
    ["height"] = "500px",
    ["minHeight"] = "200px",
    ["language"] = "en-US"
})" />
```

Refer to [TOAST UI Editor docs](https://nhn.github.io/tui.editor/latest/) for available options.

### Asset Hosting

By default, bundled assets are loaded from `/_content/WrappedToast/lib/toastui-editor/`. If your host loads TOAST UI Editor assets globally, set `LoadAssets="false"` to avoid duplicate loads:

```razor
<ToastUIEditor LoadAssets="false" />
```

### Styling

The components render as plain `<div>` elements. Style them with CSS or inline styles:

```razor
<ToastUIEditor InitialStyle="display:block;width:100%;height:400px;border:1px solid #ccc;" />
```

Dark theme is automatically available; toggle it with the `[data-theme="dark"]` attribute on a parent element.

---

## Links

- **TOAST UI Editor Repository:** https://github.com/nhn/tui.editor
- **TOAST UI Editor Documentation:** https://nhn.github.io/tui.editor/latest/
- **TOAST UI Editor v3.2.2 Release:** https://github.com/nhn/tui.editor/releases/tag/3.2.2
- **MudBlazor:** https://mudblazor.com/

---

## License

MIT — See [LICENSE](./LICENSE) and [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md).

WrappedToast is licensed under MIT to match [TOAST UI Editor's license](https://github.com/nhn/tui.editor/blob/master/LICENSE).

---

## Contributing

Issues and pull requests are welcome. Please ensure all tests pass before submitting:

```bash
dotnet test
```

## Changelog

See [CHANGELOG.md](./CHANGELOG.md) for release history.
