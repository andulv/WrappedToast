---
type: task
description: "Task 003-05 — Tighten and extend .agents/project.instructions.md"
status: implemented
created: 2026-06-14T19:44:10+02:00
updated: 2026-06-14T20:11:00+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`

Read current file:
- `.agents/project.instructions.md`

## Objective

Reduce verbosity, remove duplication, and add missing information in `project.instructions.md` so agents have complete guidance without reading source first.

## Scope

Edits to `.agents/project.instructions.md` only. No other files.

## Steps

1. **Add `samples/ToastUIEditorAndViewerSamples`** to Project Shape section (after the sample host line).
2. **Add `FrontMatterPanel` and `TextContentWithFrontMatter`** to Project Shape as "public helper types for front-matter parsing and display."
3. **Add `toastui-loader.js`** to Implementation Guidelines: "The bundled `toastui-loader.js` (`src/WrappedToast/wwwroot/toastui-loader.js`) handles lazy-loading of vendored TOAST UI CSS/JS. Do not bypass or duplicate this loader."
4. **Collapse the "Do not put X" list** (lines 29–33) into one rule: "Do not add MudBlazor dependencies, product-specific transforms, clipboard logic, front-matter logic, host navigation, or app workflow to the thin wrappers."
5. **Merge the duplicate WrappedToast-owns-MudBlazor point**: replace lines 34–37 with a single statement: "`WrappedToast` owns higher-level behavior (front matter, toolbar commands, copy/export variants, host-facing convenience APIs) and is the only component that should take MudBlazor dependencies."
6. **Add CSS isolation note** to Implementation Guidelines: "Components use Blazor CSS isolation (`.razor.css` files with `::deep` selectors). Preserve this pattern when adding styles."
7. **Add bUnit note** next to the test command: "Tests are JS-less bUnit smoke tests (no browser required)."
8. **Add mutability caution** to Implementation Guidelines: "`EditorOptions` and `ViewerOptions` on `WrappedToast` are mutable shared dictionaries. Mutating them from outside the component affects the live instance."

## Verification

- `project.instructions.md` contains `samples/ToastUIEditorAndViewerSamples`, `FrontMatterPanel`, `TextContentWithFrontMatter`, `toastui-loader.js`, CSS isolation note, bUnit note, `EditorOptions`/`ViewerOptions` caution
- `project.instructions.md` has no duplicate WrappedToast-owns-MudBlazor lines
- The "Do not put X" list is collapsed to a single rule
- File is shorter or equal in length to the original

---

Everything above this line is the task specification. Everything below is the
execution record.

# Execution

## Executor Notes
By: @ 

## Reviewer Verification
By: @ 

## Review Notes
By: @ 
