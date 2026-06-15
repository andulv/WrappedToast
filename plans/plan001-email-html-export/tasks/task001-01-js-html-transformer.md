---
type: task
description: "Task 001-01 — Implement email-safe HTML transformer in toastui-loader.js"
status: not-started
created: 2026-06-14T15:48:13+02:00
updated: 2026-06-14T15:48:13+02:00
---
## Required Context

Load and follow these skills:
- `plan-task-standards`

Read before starting:
- `submodules/WrappedToast/src/WrappedToast/wwwroot/toastui-loader.js`
- `submodules/WrappedToast/src/WrappedToast/ToastUIEditor.razor.js`
- `submodules/WrappedToast/src/WrappedToast/ToastUIEditorViewer.razor.js`
- `submodules/WrappedToast/src/WrappedToast/ToastUIEditorCore.cs`

## Objective

Add a post-processing step in the JavaScript layer so that HTML copied from the WrappedToast editor or viewer is self-contained, email-client friendly, and visually consistent with the on-screen preview.

## Scope

Included:
- Add a new `toEmailHtml(element)` helper (or equivalent) in `toastui-loader.js` that clones the rendered DOM, removes `data-nodeid` attributes, removes empty `<p>` elements, and injects inline CSS.
- Update `getToastUiHtml` to route both the `instance.getHTML()` string path and the DOM fallback through the post-processor.
- Ensure both the editor and viewer `copyHtmlToClipboard` functions produce the transformed HTML.

Excluded:
- Changing the Toast UI Editor library files.
- Changing the live viewer/editor DOM or on-screen styling.
- Adding new C# methods or UI buttons.

## Steps

1. In `toastui-loader.js`, create a helper function that accepts a DOM element or an HTML string and returns a cleaned, email-safe HTML string.
2. Remove all `data-nodeid` attributes from the clone.
3. Remove empty `<p>` elements that do not add visual value.
4. Apply inline styles:
   - Base container: `font-family`, `font-size`, `line-height`, `color`.
   - `h1`/`h2`/`h3`–`h6`: explicit `font-size`, `font-weight`, `margin`, `color`.
   - `p`: `margin`.
   - `ul`/`ol`: `margin`, `padding-left`.
   - `li`: `margin`; nested `li > p` should have `margin: 0` to avoid double spacing.
   - `hr`: `border`, `border-top`, `margin`.
   - `table`: `border-collapse`, `width`, `margin`; add `border="1"` attribute for older clients.
   - `th`/`td`: `border`, `padding`, `text-align`, `vertical-align`; `th` should have a light background and bold font.
   - `strong`/`em`: ensure `font-weight`/`font-style`.
5. For the `instance.getHTML()` code path, parse the string with `DOMParser`, apply the same transformations, and serialize it back.
6. Update `getToastUiHtml` to call the post-processor for both code paths.
7. Keep `defaultHtmlResolver` using the new transformer.
8. Build the `WrappedToast` project to confirm there are no JS-module syntax errors that break the Blazor build pipeline.

## Verification

- `dotnet build submodules/WrappedToast/src/WrappedToast/WrappedToast.csproj` exits 0.
- In the browser, calling the sample app's "Copy as HTML" action and reading the clipboard via a temporary script shows:
  - No `data-nodeid` attributes.
  - `<table>` with `style="border-collapse: collapse; ..."`.
  - `<h1>` with an explicit `style` attribute containing `font-size`.
  - No empty `<p></p>` elements.

---

# Execution

## Executor Notes

By:  @

## Executor Verification

By:  @

## Reviewer Verification

By:  @

## Review Notes

By:  @
