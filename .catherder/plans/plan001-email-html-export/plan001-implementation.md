---
type: plan-implementation
description: "Plan 001 - Implement email-safe HTML export for WrappedToast"
status: active
created: 2026-06-14T15:48:13+02:00
updated: 2026-06-14T15:48:13+02:00
---
# Plan 001 Implementation — Email-Client Friendly HTML Export for WrappedToast

## 0. Required Context

- Spec: `plan001-spec.md`
- `plan-task-standards` skill
- `nuget-manager` skill (if build verification requires package changes; not expected here)
- Source files to modify:
  - `submodules/WrappedToast/src/WrappedToast/wwwroot/toastui-loader.js`
  - `submodules/WrappedToast/src/WrappedToast/ToastUIEditor.razor.js` (verify wiring only)
  - `submodules/WrappedToast/src/WrappedToast/ToastUIEditorViewer.razor.js` (verify wiring only)
- Sample app to verify:
  - `submodules/WrappedToast/samples/WrappedToast.SampleHost/Components/Pages/WrappedToastSample.razor`

## 1. Tasks

Allowed task statuses: not-started, in-progress, blocked, implemented, reviewed, completed.

| Status | Task |
|---|---|
| `not-started` | [Task P001-T01: Implement email-safe HTML transformer in toastui-loader.js](tasks/task001-01-js-html-transformer.md) |
| `not-started` | [Task P001-T02: Verify exported HTML in sample app and webmail paste](tasks/task001-02-verify-html-output.md) |

## 2. Task Parallelism

Tasks are sequential:
- T01 must be completed before T02 can run, because T02 verifies the HTML produced by T01.

## 3. Acceptance Criteria

- [ ] Clicking "Copy as HTML" from the WrappedToast sample app produces HTML with inline styles on headings, paragraphs, lists, tables, table cells, and horizontal rules.
- [ ] The exported HTML contains no `data-nodeid` attributes.
- [ ] The exported HTML contains no empty `<p></p>` elements left over from markdown blank lines.
- [ ] Tables in the exported HTML have `border-collapse: collapse`, visible borders, and cell padding.
- [ ] The exported HTML renders with borders and reasonable spacing when pasted into the Domeneshop webmail compose window (or another rich-text editor) without relying on the WrappedToast stylesheet.
- [ ] Existing viewer/editor rendering on the sample page remains unchanged.
- [ ] The build of the `WrappedToast` project and the `WrappedToast.SampleHost` project succeeds.
