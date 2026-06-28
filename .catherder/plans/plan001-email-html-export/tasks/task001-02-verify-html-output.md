---
type: task
description: "Task 001-02 — Verify exported HTML in sample app and webmail paste"
status: not-started
created: 2026-06-14T15:48:13+02:00
updated: 2026-06-14T15:48:13+02:00
---
## Required Context

Load and follow these skills:
- `plan-task-standards`

Prerequisite:
- Task P001-T01 completed and implemented.

## Objective

Confirm that the transformed HTML looks professional when pasted into an external rich-text editor, and that the in-app preview is unchanged.

## Scope

Included:
- Run the `WrappedToast.SampleHost` application.
- Copy HTML from the WrappedToast sample page and inspect it.
- Paste the copied HTML into the Domeneshop webmail compose window (or another rich-text editor) and compare visual result with the original preview.
- Document any remaining destination-side normalization issues that are outside our control.

Excluded:
- Fixing how the destination email client normalizes pasted HTML (e.g., TinyMCE inserting `<p><br></p>`).
- Adding automated UI tests.

## Steps

1. Build and run the sample host:
   - `dotnet build submodules/WrappedToast/samples/WrappedToast.SampleHost/WrappedToast.SampleHost.csproj`
   - Run the sample host and navigate to `/wrappedtoast-sample`.
2. In the browser, open the WrappedToast sample page and click "Copy as HTML".
3. Inspect the clipboard content using a browser console snippet or by pasting into a plain text editor. Verify:
   - Inline styles are present on headings, paragraphs, lists, tables, cells, and horizontal rules.
   - No `data-nodeid` attributes remain.
   - No empty `<p></p>` elements remain.
4. Open the Domeneshop webmail compose window (or another rich-text editor) and paste the HTML.
5. Compare the pasted result with the original WrappedToast preview. Take note of:
   - Whether table borders and cell padding are visible.
   - Whether heading sizes and paragraph spacing look professional.
   - Any remaining issues caused by the destination editor (e.g., TinyMCE's `<p><br></p>` spacers or `<hr>` wrappers).
6. If the visual result is not acceptable, record the specific gaps and decide whether they can be addressed by further adjusting the source HTML or are destination-side limitations.

## Verification

- `dotnet build submodules/WrappedToast/samples/WrappedToast.SampleHost/WrappedToast.SampleHost.csproj` exits 0.
- The sample page at `/wrappedtoast-sample` renders the same as before the changes.
- Manual check: pasted HTML in the webmail compose window shows table borders, heading sizes, and paragraph spacing that closely match the original WrappedToast preview.
- Manual check: any remaining `<p><br></p>` spacers or `<hr>` wrappers are identified as coming from the destination editor, not the exported HTML.

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
