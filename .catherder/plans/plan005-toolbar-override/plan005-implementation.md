---
type: plan-implementation
description: "Plan 005 - support an in-place host toolbar override"
status: completed
created: 2026-08-01T11:57:37+02:00
updated: 2026-08-01T12:07:00+02:00
---
# Plan 005 Implementation — Toolbar Override

## 0. Required Context

- Spec: `plan005-spec.md`
- `plan-task-standards`
- `src/WrappedToast/WrappedToast.razor(.cs)`
- `tests/WrappedToast.Tests/ComponentSmokeTests.cs`
- `README.md` and `doc/wrappedtoast.md`

## 1. Tasks

Allowed task statuses: not-started, in-progress, blocked, implemented, reviewed, completed.

| Status | Task |
|---|---|
| `completed` | [Task P005-T01: add and document a general toolbar override](tasks/task005-01-toolbar-override.md) |

## 2. Task Parallelism

T01 is the only task. It must be reviewed and published before parent Plan 147 task
T07 updates the CatHerder submodule pin.

## 3. Acceptance Criteria

- [x] An optional host fragment replaces the normal toolbar in the existing toolbar slot.
- [x] The default toolbar and `ToolbarExtras` remain unchanged without an override.
- [x] bUnit tests cover default and override output; package build and tests pass.
- [x] The API documentation describes precedence and host-neutral intent.
