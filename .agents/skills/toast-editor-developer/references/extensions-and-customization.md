---
type: reference
description: "TOAST UI Editor extension points: events, hooks, plugins, custom renderers, widgets, i18n, themes, and sanitizer considerations"
snapshot_date: 2026-06-14
sources:
  - https://ui.toast.com/tui-editor/
  - https://nhn.github.io/tui.editor/latest/ToastUIEditorCore/
  - https://raw.githubusercontent.com/nhn/tui.editor/master/docs/en/custom-html-renderer.md
  - https://raw.githubusercontent.com/nhn/tui.editor/master/docs/en/widget.md
  - https://github.com/nhn/tui.editor
---
# Extensions And Customization

## Events And Hooks

Use constructor `events` for lifecycle and interaction callbacks when binding at
creation time. Use `on(type, handler)` and `off(type)` when binding later.

Common events include:

- `load`
- `change`
- `caretChange`
- `focus`
- `blur`
- `keydown`
- `keyup`
- `beforePreviewRender`
- `beforeConvertWysiwygToMarkdown`

Use `hooks` or `addHook(type, handler)` for hook-style integrations such as
image upload behavior. Remove hooks with `removeHook(type)` when replacing or
disposing dynamic integrations.

## Plugins

The `plugins` option accepts plugin functions or `[plugin, options]` pairs.
Official plugin packages include chart, code syntax highlight, color syntax,
table merged cell, and UML. Plugins often require their own CSS and sometimes
their own peer data, such as Prism language definitions for syntax highlighting.

Check plugin README files before wiring a plugin into bundler or CDN code. Do
not assume the core editor CSS covers plugin UI.

## Custom HTML Rendering

Use `customHTMLRenderer` to customize Markdown AST node conversion to HTML
tokens. Renderer functions receive a node and context. They return token objects
such as:

- `openTag`
- `closeTag`
- `text`
- `html`

Prefer `text` tokens for untrusted text because they are escaped. Use `html`
tokens only when raw HTML output is intentional and sanitized.

Useful context capabilities:

- `entering`: distinguishes entering and leaving non-leaf nodes.
- `origin()`: calls the original renderer for the current node.
- `getChildrenText(node)`: gets textual child content.
- `skipChildren()`: prevents traversal of children when the current renderer
  fully handles them.

Use `customHTMLSanitizer` when the application has specific HTML safety needs.

## Widgets

Use `addWidget(node, style, pos)` for temporary popup-like DOM near the current
selection or cursor. This does not become document content.

Use `widgetRules` for inline widgets that replace matching text patterns in the
editor content. Rules contain:

- `rule`: regular expression to match text.
- `toDOM`: function returning the DOM node to render.

For insertion flows such as mentions, combine selection APIs with
`insertText()` or `replaceSelection()` and text that matches the widget rule.

## Toolbar, Commands, And Language

Use `toolbarItems`, `insertToolbarItem()`, and `removeToolbarItem()` for toolbar
customization. Use `addCommand(type, name, command)` and `exec(name, payload)`
for command-based behavior.

Use `Editor.setLanguage(code, data)` for custom UI language data. Confirm the
language bundle or custom data is loaded before constructing localized editors.

## Themes

Dark theme support requires the relevant theme CSS. Set the `theme` option only
after ensuring the CSS is loaded by the host page or bundler.
