---
type: plan-spec
description: "Plan 005 - support an in-place host toolbar override"
status: ready
created: 2026-08-01T11:57:37+02:00
updated: 2026-08-01T11:57:37+02:00
---
# Plan 005 Spec — Toolbar Override

## 0. Required Context

- `plan-task-standards`
- `src/WrappedToast/WrappedToast.razor(.cs)`
- `tests/WrappedToast.Tests/ComponentSmokeTests.cs`
- `README.md` and `doc/wrappedtoast.md`
- Parent integration: `../../../../../../.catherder/plans/plan147-editor-dirty-state-and-file-change-notifications/`

## 1. Goal

Give hosts one general render slot that replaces WrappedToast's normal toolbar in the
same layout position.

## 2. Context / Why

CatHerder's external-change conflict warning currently renders as a separate alert above
the editor body. It shifts content and leaves the normal WrappedToast toolbar visible.
The conflict state belongs to the host, but the reusable component should expose one
neutral toolbar replacement point.

## 3. What We Want To Achieve (Outcomes)

- A host can render an alternative toolbar in the normal toolbar slot.
- Without an override, current WrappedToast toolbar behaviour is unchanged.
- The package remains unaware of conflict state, file systems, or navigation.
- Existing `ToolbarExtras` behaviour remains available in the default toolbar.

## 4. Key Principles / Constraints

- Expose one optional general `RenderFragment` override, not conflict-specific parameters.
- The override replaces the whole normal toolbar; do not stack a second toolbar or create
  a collection of narrow slots.
- Retain `ToolbarExtras` because it serves the default toolbar and existing sample hosts.
- Cover both default and override rendering with stable bUnit assertions.

## 5. Out of Scope

- CatHerder warning copy, actions, or conflict decisions.
- Toolbar visual redesign, editor state changes, and JavaScript changes.
- Removing `ToolbarExtras` or changing existing consumers.

## 6. Implementation Notes

Conditionally render a single outer toolbar container: render the supplied override when
present, otherwise render the current default content. Keep the package API and docs clear
about override precedence over the default toolbar.

## 7. Open Questions

1. Existing `ToolbarExtras` — *(resolved: retain it for the default toolbar; an override
   replaces the entire default toolbar, including extras.)*
2. Conflict knowledge — *(resolved: none in this package; the host supplies arbitrary
   toolbar content.)*
3. Layout position — *(resolved: use the existing toolbar container rather than a new
   alert/body slot.)*
