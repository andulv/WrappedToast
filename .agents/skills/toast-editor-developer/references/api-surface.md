---
type: reference
description: "TOAST UI Editor, Viewer, and core API surface notes with options, methods, and asymmetries"
snapshot_date: 2026-06-14
sources:
  - https://nhn.github.io/tui.editor/latest/
  - https://nhn.github.io/tui.editor/latest/ToastUIEditor/
  - https://nhn.github.io/tui.editor/latest/ToastUIEditorCore/
  - https://nhn.github.io/tui.editor/latest/ToastUIEditorViewer/
---
# API Surface

## Version Snapshot

The official API site snapshot used for this reference identifies version
`3.2.2`. Re-check upstream for exact signatures before writing code that depends
on edge behavior.

## Core Options

Common constructor options include:

| Option | Use |
|---|---|
| `el` | Required container element. |
| `height` | Editor/viewer height, such as `300px`, `100%`, or `auto`. |
| `minHeight` | Editor minimum content height. |
| `initialValue` | Initial Markdown string. |
| `initialEditType` | Editor starting mode: `markdown` or `wysiwyg`. |
| `previewStyle` | Markdown preview layout: `tab` or `vertical`. |
| `events` | Event callbacks such as `load`, `change`, `focus`, `blur`, key events, and conversion hooks. |
| `hooks` | Hook callbacks such as `addImageBlobHook`. |
| `language` | UI language code. |
| `usageStatistics` | Telemetry toggle. |
| `toolbarItems` | Toolbar configuration. |
| `hideModeSwitch` | Hide the mode switch UI. |
| `plugins` | Plugin functions or `[plugin, options]` pairs. |
| `placeholder` | Editable element placeholder. |
| `linkAttributes` | Attributes applied to rendered links. |
| `customHTMLRenderer` | Markdown AST node to HTML-token conversion overrides. |
| `customMarkdownRenderer` | WYSIWYG node to Markdown conversion overrides. |
| `customHTMLSanitizer` | HTML sanitizer function. |
| `frontMatter` | Enable front matter handling. |
| `widgetRules` | Inline widget replacement rules. |
| `theme` | Theme name, such as dark theme when CSS is included. |
| `autofocus` | Focus editor on creation. |

Viewer supports a smaller option set. Treat documented Viewer options as the
source of truth instead of copying the full Editor option object.

## Editor Methods

High-use Editor methods:

- Content: `getMarkdown()`, `setMarkdown(markdown, cursorToEnd)`, `getHTML()`,
  `setHTML(html, cursorToEnd)`.
- Mode/UI: `changeMode(mode, withoutFocus)`, `isMarkdownMode()`,
  `isWysiwygMode()`, `getCurrentPreviewStyle()`, `changePreviewStyle(style)`,
  `show()`, `hide()`, `focus()`, `blur()`.
- Selection/cursor: `getSelection()`, `setSelection(start, end)`,
  `getSelectedText(start, end)`, `replaceSelection(text, start, end)`,
  `deleteSelection(start, end)`, `moveCursorToStart(focus)`,
  `moveCursorToEnd(focus)`.
- Events/hooks: `on(type, handler)`, `off(type)`, `addHook(type, handler)`,
  `removeHook(type)`.
- Commands/widgets: `exec(name, payload)`, `addCommand(type, name, command)`,
  `addWidget(node, style, pos)`, `replaceWithWidget(start, end, text)`.
- Layout: `getHeight()`, `setHeight(height)`, `getMinHeight()`,
  `setMinHeight(minHeight)`, `getScrollTop()`, `setScrollTop(value)`.
- Lifecycle: `destroy()`.

The `ToastUIEditor` class also exposes toolbar methods:

- `insertToolbarItem(indexInfo, item)`
- `removeToolbarItem(itemName)`

Static methods:

- `Editor.factory(options)` returns an editor or viewer depending on options.
- `Editor.setLanguage(code, data)` registers language data.

## Viewer Methods

Documented Viewer methods:

- `setMarkdown(markdown)`
- `isViewer()`
- `isMarkdownMode()`
- `isWysiwygMode()`
- `on(type, handler)`
- `off(type)`
- `addHook(type, handler)`
- `destroy()`

Viewer is not a read API. Do not expect documented `getMarkdown()`, `getHTML()`,
or `setHTML()` methods on Viewer.

## Editor vs Viewer Asymmetry

| Need | Use |
|---|---|
| User edits Markdown/WYSIWYG content | `Editor` |
| User only reads rendered Markdown | `Viewer` |
| Get Markdown from current surface | `Editor.getMarkdown()` |
| Get rendered HTML from current surface | `Editor.getHTML()` |
| Replace displayed Markdown in read-only surface | `Viewer.setMarkdown()` |
| Create viewer from editor bundle | `Editor.factory({ viewer: true, ... })` |

## Selection Coordinates

Selection and position APIs are mode-dependent:

- Markdown mode uses nested line/column arrays, e.g.
  `[[startLine, startColumn], [endLine, endColumn]]`.
- WYSIWYG mode uses offsets, e.g. `[startOffset, endOffset]`.

When code manipulates positions, either force a known mode first or branch based
on `isMarkdownMode()` / `isWysiwygMode()`.
