---
type: plan-implementation
description: "Plan 003 - Fix factual errors, fill README gaps, tighten .agents/ instructions"
status: active
created: 2026-06-14T19:44:10+02:00
updated: 2026-06-14T19:44:10+02:00
---
# Plan 003 Implementation — Documentation Review and Cleanup

## 0. Required Context

- Spec: `plan003-spec.md`
- `plan-task-standards` skill
- Current files: `README.md`, `.agents/project.instructions.md`
- The `toast-editor-developer` skill is now a generic TOAST UI reference (no WrappedToast overlap; not modified by this plan)
- Source code under `src/WrappedToast/` (read-only reference — do not modify)

## 1. Tasks

Allowed task statuses: not-started, in-progress, blocked, implemented, reviewed, completed.

| Status | Task |
|---|---|
| `implemented` | [Task P003-T01: Fix factual errors in README.md](tasks/task003-01-fix-readonly-factual-errors.md) |
| `implemented` | [Task P003-T02: Add thin-wrapper parameter table and asset-loading explanation to README.md](tasks/task003-02-readme-thin-wrapper-docs.md) |
| `implemented` | [Task P003-T03: Create doc/api-reference.md and link from README.md](tasks/task003-03-api-reference-file.md) |
| `implemented` | [Task P003-T04: Add HandleSaveAsync example to README.md](tasks/task003-04-readme-handlesave-example.md) |
| `implemented` | [Task P003-T05: Tighten and extend .agents/project.instructions.md](tasks/task003-05-tighten-project-instructions.md) |

## 2. Task Parallelism

- T01–T04 all modify README.md and should run **sequentially** to avoid merge conflicts.
- T05 modifies `.agents/project.instructions.md` only and can run in parallel with T01–T04.
- T03 creates a new file `doc/api-reference.md` and then links it from README; the README edit should happen after T01 and T02 to avoid conflicts.

Recommended order: T01 → T02 → T03 → T04, with T05 any time.

## 3. Acceptance Criteria

- [x] README.md contains no factual inaccuracies (no `<HeadContent>` claim, no redundant `["viewer"] = "true"`, programmatic API attributed to `WrappedToast`)
- [x] README.md has a thin-wrapper parameter table and explains asset auto-loading
- [x] `doc/api-reference.md` exists with `ToastUIEditor` and `ToastUIEditorViewer` method tables, linked from README.md
- [x] README.md shows a trivial `HandleSaveAsync` callback example
- [x] `.agents/project.instructions.md` includes `FrontMatterPanel`, `TextContentWithFrontMatter`, `toastui-loader.js`, `samples/ToastUIEditorAndViewerSamples`, CSS isolation note, bUnit note, and `EditorOptions`/`ViewerOptions` mutability caution
- [x] `.agents/project.instructions.md` has no duplicate or overly verbose boundary-rule lines
- [x] `dotnet build src/WrappedToast/WrappedToast.csproj` still succeeds
- [ ] `dotnet test tests/WrappedToast.Tests/WrappedToast.Tests.csproj` still passes
