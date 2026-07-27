# Changelog

All notable changes to **WrappedToast** are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Dirty tracking: sticky `IsDirty` + `OnDirtyChanged`, a warning-coloured Save button
  while dirty, a `beforeunload` prompt while dirty, and `MarkDirty()` for hosts whose
  save was rejected. Cleared on save, on Cancel, and on content load; undoing back to
  the saved text does not clear it.
- `ToastUIEditor.OnContentChanged` (+ `SetChangeSuspendedAsync`) and
  `FrontMatterPanel.OnEdited` — the change signals dirty tracking is built on.
- `SetContent(content, force: true)` to re-apply content that is byte-identical to what
  was last applied (discard-my-edits reload).

### Fixed

- `OnParametersSet` no longer re-pushes `Content` into the editor on every parent
  re-render, which discarded whatever the user had typed since the load.

## [0.1.0-preview.1]

### Added

- Initial release as a standalone package wrapping [TOAST UI Editor](https://github.com/nhn/tui.editor) v3.2.2.
- `ToastUIEditor` — Blazor component for editing markdown content with TOAST UI Editor.
- `ToastUIEditorViewer` — Blazor component for viewing markdown content with TOAST UI Editor.
- `ToastUIEditorCore` — Shared base class for editor and viewer components.
- `WrappedToast` — Higher-level component combining editor/viewer with MudBlazor toolbar (Edit/Save/Cancel) and optional YAML front-matter display.
- `WrappedToast.ToolbarExtras` `RenderFragment` parameter to inject host-specific buttons without coupling the package to host navigation.
- TOAST UI Editor 3.2.2 assets vendored into `wwwroot/lib/toastui-editor/` and served as static web assets at `_content/WrappedToast/lib/toastui-editor/...`. CDN is not required.

### Changed

- Licensed under MIT to match [TOAST UI Editor's license](https://github.com/nhn/tui.editor/blob/master/LICENSE).
- Static asset paths: `./_content/WrappedToast/...`
- Root namespace: `WrappedToast`

### Removed

- Not applicable for initial release.
