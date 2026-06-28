---
type: task
description: "Task 003-03 — Create doc/api-reference.md and link from README.md"
status: implemented
created: 2026-06-14T19:44:10+02:00
updated: 2026-06-14T20:08:00+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`

Read these source files for the method signatures:
- `src/WrappedToast/ToastUIEditor.razor` (public methods)
- `src/WrappedToast/ToastUIEditorViewer.razor` (public methods)
- `src/WrappedToast/ToastUIEditorCore.cs` (shared base, `InitialStyle` and `Options` params)
- `src/WrappedToast/WrappedToast.razor.cs` (WrappedToast public methods)

## Objective

Create a standalone API reference document under `doc/` listing the public methods of `ToastUIEditor`, `ToastUIEditorViewer`, and `WrappedToast`, and link to it from README.md.

## Scope

- Create `doc/api-reference.md` with method tables for each component.
- Add a link to `doc/api-reference.md` from the README thin-wrapper section and WrappedToast section.
- Do not inline the full method tables in README.

## Steps

1. Create `doc/api-reference.md` with three sections:
   - **ToastUIEditor** — table of public methods (name, return type, description) sourced from `ToastUIEditor.razor`
   - **ToastUIEditorViewer** — table of public methods sourced from `ToastUIEditorViewer.razor`
   - **WrappedToast** — table of public methods sourced from `WrappedToast.razor.cs` (grouped: content read, editor manipulation, front-matter)
   - Each section lists the parameters from the relevant component (already partially in README for WrappedToast; include them here too for completeness).
2. In README.md, in the "ToastUIEditor / ToastUIEditorViewer - Features" section, add: "See [API Reference](doc/api-reference.md) for the full method list."
3. In README.md, after the WrappedToast Parameters table, add: "See [API Reference](doc/api-reference.md) for the full method list including programmatic editor APIs."

## Verification

- `doc/api-reference.md` exists and contains method tables for all three components
- `doc/api-reference.md` method names match the actual public methods in source
- README.md links to `doc/api-reference.md` from both the thin-wrapper and WrappedToast sections
- `dotnet build src/WrappedToast/WrappedToast.csproj` succeeds (doc file is non-code; should be no impact)

---

Everything above this line is the task specification. Everything below is the
execution record.

# Execution

## Executor Notes
By: @ 

## Executor Verification
By: @ 

## Reviewer Verification
By: @ 

## Review Notes
By: @ 
