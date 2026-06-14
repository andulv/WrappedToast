---
type: reference
description: "TOAST UI Editor setup, component choice, npm/CDN imports, and basic Editor/Viewer creation"
snapshot_date: 2026-06-14
sources:
  - https://ui.toast.com/tui-editor/
  - https://nhn.github.io/tui.editor/latest/
  - https://github.com/nhn/tui.editor
  - https://raw.githubusercontent.com/nhn/tui.editor/master/docs/en/getting-started.md
  - https://raw.githubusercontent.com/nhn/tui.editor/master/docs/en/viewer.md
---
# Usage And Components

## What TOAST UI Editor Provides

TOAST UI Editor is a Markdown editor with Markdown and WYSIWYG modes. It targets
Markdown document production and editing, with features such as live preview,
scroll sync, syntax highlighting, toolbar UI, dark theme support, plugins,
i18n, widgets, and custom rendering.

The package exposes two practical surfaces:

- `Editor`: full editing UI for Markdown and WYSIWYG workflows.
- `Viewer`: read-only Markdown display. Use it when the user should not edit and
  the page does not need the full editor.

## Package And CDN

Preferred package:

```sh
npm install --save @toast-ui/editor
```

Editor imports:

```js
import Editor from '@toast-ui/editor';
import '@toast-ui/editor/dist/toastui-editor.css';
```

Viewer imports:

```js
import Viewer from '@toast-ui/editor/dist/toastui-editor-viewer';
import '@toast-ui/editor/dist/toastui-editor-viewer.css';
```

Browser namespace usage is available through CDN bundles. When using CDN, include
the matching CSS and JS files from `https://uicdn.toast.com/editor/<version>/`.
Prefer a concrete version over `latest` for production.

## Basic Editor

Create a container element and pass it as `el`.

```html
<div id="editor"></div>
```

```js
const editor = new Editor({
  el: document.querySelector('#editor'),
  height: '600px',
  initialEditType: 'markdown',
  previewStyle: 'vertical',
  initialValue: '# Hello',
  usageStatistics: false
});
```

Common initial options:

- `el`: container element.
- `height`: string such as `300px`, `100%`, or `auto`.
- `initialValue`: Markdown string.
- `initialEditType`: `markdown` or `wysiwyg`.
- `previewStyle`: `tab` or `vertical`.
- `usageStatistics`: set `false` when telemetry is not wanted.

## Basic Viewer

Use the viewer package when only rendering Markdown.

```html
<div id="viewer"></div>
```

```js
const viewer = new Viewer({
  el: document.querySelector('#viewer'),
  initialValue: '# Hello'
});
```

Alternative factory route:

```js
const viewer = Editor.factory({
  el: document.querySelector('#viewer'),
  viewer: true,
  initialValue: '# Hello'
});
```

Do not load full editor and standalone viewer bundles together unless the host
build deliberately needs both. The editor bundle can create a viewer through
`Editor.factory({ viewer: true })`.
