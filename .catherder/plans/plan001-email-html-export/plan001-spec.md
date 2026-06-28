---
type: plan-spec
description: "Plan 001 - Make WrappedToast HTML export email-client friendly and visually consistent"
status: ready
created: 2026-06-14T15:48:13+02:00
updated: 2026-06-14T15:48:13+02:00
---
# Plan 001 Spec — Email-Client Friendly HTML Export for WrappedToast

## 0. Required Context

- `plan-task-standards` skill
- `WrappedToast` component source: `submodules/WrappedToast/src/WrappedToast/`
- Key files:
  - `submodules/WrappedToast/src/WrappedToast/wwwroot/toastui-loader.js`
  - `submodules/WrappedToast/src/WrappedToast/ToastUIEditor.razor.js`
  - `submodules/WrappedToast/src/WrappedToast/ToastUIEditorViewer.razor.js`
  - `submodules/WrappedToast/src/WrappedToast/ToastUIEditorCore.cs`
- Sample app: `submodules/WrappedToast/samples/WrappedToast.SampleHost/`

## 1. Goal

Make the HTML copied from the WrappedToast component render as professionally as the in-app preview when pasted into email clients, word processors, and similar rich-text consumers. The exported HTML must be self-contained, simple, and compatible with common mail-client rendering engines.

## 2. Context / Why

The current HTML export is Toast UI Editor's internal DOM representation (from `.toastui-editor-contents` or `instance.getHTML()`). It relies on the `toastui-editor.min.css` stylesheet for visual styling. When pasted into an external rich-text editor or email client, that stylesheet is not present, so tables lose borders, headings lose size/margin, and paragraphs collapse into uneven spacing. Additionally, email clients re-normalize the markup (e.g., TinyMCE inserts `<p><br></p>` spacers and wraps `<hr>` in `<div contenteditable="false">`), which amplifies the visual degradation.

## 3. What We Want To Achieve (Outcomes)

1. Copied HTML is self-contained and uses inline styles instead of relying on external CSS classes.
2. Tables render with visible borders, cell padding, and consistent alignment in email clients.
3. Headings, paragraphs, lists, and horizontal rules have explicit, professional-looking margins and font sizes.
4. Editor-only artifacts (`data-nodeid` attributes and empty placeholder paragraphs) are removed from the exported HTML.
5. The change applies to both the editor and viewer copy paths without changing the on-screen rendering or editing behavior.

## 4. Key Principles / Constraints

- Keep the generated HTML simple and widely compatible with email clients and word processors.
- Use inline CSS only; do not depend on external stylesheets or `<style>` blocks.
- Do not alter the Toast UI Editor library itself or the live viewer/editor DOM.
- Preserve the existing markdown-to-HTML conversion behavior; only post-process the generated HTML before clipboard export.
- Changes must be minimal and maintainable in `toastui-loader.js` and related JS module files.
- Work for both the `Copy as HTML` menu action and any programmatic `GetHtmlAsync()` calls.

## 5. Out of Scope

- Controlling how the destination email client re-normalizes pasted HTML (e.g., TinyMCE adding `<p><br></p>` or wrapping `<hr>`). We will only improve the source HTML we send to the clipboard.
- Adding new export formats (PDF, DOCX, etc.) or new UI buttons.
- Changing the markdown editor's live rendering or the viewer's on-screen styling.
- Refactoring the C# interop layer beyond wiring to the new HTML post-processor.

## 6. Implementation Notes

- Add a post-processing function in `toastui-loader.js` that clones the `.toastui-editor-contents` DOM, strips `data-nodeid`, removes empty paragraphs, and injects inline styles for headings, paragraphs, lists, tables, cells, and horizontal rules.
- If Toast UI's `instance.getHTML()` returns a string, use `DOMParser` to apply the same transformations to that string.
- Wire the post-processor into `getToastUiHtml` so it is used by both the editor and viewer modules before the HTML reaches the clipboard.
- Consider wrapping the result in a single `<div>` with base font styles to help email clients that strip styles from bare child elements.
- Verify by copying HTML from the sample app and inspecting it in a text editor or browser console; optionally paste it into the Domeneshop webmail compose window to confirm visual improvement.

## 7. Open Questions

1. Should we provide a separate "plain HTML" copy vs. always using email-safe HTML? (resolved: always use email-safe HTML for the existing "Copy as HTML" action; it is the primary use case.)
2. Should `data-nodeid` attributes be retained in any export scenario? (resolved: no, they are editor-internal and should be removed from all exported HTML.)
3. How should we verify the output — only browser inspection, or add an automated test? (resolved: browser inspection of the sample app plus a simple console check of the copied HTML is sufficient for this plan. Automated tests can be added later if desired.)
