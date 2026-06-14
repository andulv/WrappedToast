---
name: toast-editor-developer
description: "Develop web applications with TOAST UI Editor html/javascript component for Markdown viewing and editing. Use when you need to understand, plan, review, or write code that uses TOAST UI Editor, TOAST UI Editor Viewer or ToastUIEditorCore for markdown viewing or editing."
---
# Toast Editor Developer

Framework-neutral and project-agnostic skill for using the TOAST UI Editor JavaScript component in web applications. This skill covers the core Editor and Viewer APIs, including configuration, content manipulation, mode management, selection handling, events/hooks, commands/widgets, layout control, and lifecycle management.

## Workflow

1. Classify the target:
   - `Editor`: full Markdown/WYSIWYG editing surface.
   - `Viewer`: lighter read-only Markdown rendering surface.
   - `ToastUIEditorCore`: shared editor API/options documented by the API site.
2. Read only the reference needed for the task:
   - [usage-and-components.md](references/usage-and-components.md): install,
     CSS, npm/CDN setup, basic Editor and Viewer creation.
   - [api-surface.md](references/api-surface.md): options, method groups,
     selection coordinates, and Editor vs Viewer API differences.
   - [extensions-and-customization.md](references/extensions-and-customization.md):
     plugins, hooks, events, custom renderers, widgets, i18n, themes, and
     sanitizer concerns.
3. Verify against the current upstream docs when exact signatures, package
   names, or version-sensitive behavior matter.

## Rules

- Import the required TOAST UI CSS. The JavaScript constructor alone is not
  enough.
- Use `Editor` for editing and `Viewer` for read-only display unless one bundle
  choice is required by the host environment.
- Treat Editor and Viewer as related but asymmetric APIs. Do not assume a method
  exists on Viewer because it exists on Editor.
- Use `initialValue` for initial Markdown content. Use `setMarkdown()` for later
  replacement.
- Use `getMarkdown()` and `getHTML()` only on Editor. Viewer exposes
  `setMarkdown()` but not content-read APIs in the documented public surface.
- Respect mode-dependent selection coordinates: Markdown mode uses line/column
  pairs; WYSIWYG mode uses offsets.
- Prefer official options, events, hooks, and plugins before adding DOM
  workarounds.
- For custom HTML output, account for sanitization and the difference between
  escaped text tokens and raw HTML tokens.
- Verify behavior in a browser or browser-like DOM environment. Rendering,
  focus, selection, keyboard, and toolbar behavior are not reliably proven by
  static checks.

## Common Patterns

Editor via npm:

```js
import Editor from '@toast-ui/editor';
import '@toast-ui/editor/dist/toastui-editor.css';

const editor = new Editor({
  el: document.querySelector('#editor'),
  height: '500px',
  initialEditType: 'markdown',
  previewStyle: 'vertical',
  initialValue: '# Hello'
});
```

Viewer via npm:

```js
import Viewer from '@toast-ui/editor/dist/toastui-editor-viewer';
import '@toast-ui/editor/dist/toastui-editor-viewer.css';

const viewer = new Viewer({
  el: document.querySelector('#viewer'),
  initialValue: '# Hello'
});
```

Viewer through the Editor factory:

```js
import Editor from '@toast-ui/editor';

const viewer = Editor.factory({
  el: document.querySelector('#viewer'),
  viewer: true,
  initialValue: '# Hello'
});
```

## Verification

- Confirm imports and CSS match the chosen package/CDN route.
- Exercise create, update, read, destroy, and event cleanup paths.
- Test Editor and Viewer separately when both are used.
- Check Markdown mode and WYSIWYG mode separately when using selections,
  commands, or cursor APIs.
- Re-check plugin CSS/script dependencies when plugins are enabled.
