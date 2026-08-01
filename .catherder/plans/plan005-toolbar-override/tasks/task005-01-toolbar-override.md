---
type: task
description: "Task 005-01 — add and document a general toolbar override"
status: not-started
created: 2026-08-01T11:57:37+02:00
updated: 2026-08-01T11:57:37+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`
- `blazor-expert`
- `blazor-mudblazor-layout`

Read:
- `../plan005-spec.md`
- `../plan005-implementation.md`
- `src/WrappedToast/WrappedToast.razor(.cs)`
- `tests/WrappedToast.Tests/ComponentSmokeTests.cs`
- `README.md` and `doc/wrappedtoast.md`

## Objective
Expose one host-neutral render fragment that replaces WrappedToast's normal toolbar in
place, while preserving the default toolbar and `ToolbarExtras` when no override exists.

## Scope
Included:
- Add the optional toolbar override parameter and conditionally render it in the existing
  toolbar container.
- Keep the default toolbar markup and `ToolbarExtras` behaviour intact when override is
  absent.
- Add focused bUnit coverage for the default and override cases.
- Document the API, precedence, and host-neutral purpose.

Excluded:
- CatHerder conflict copy/actions, toolbar visual redesign, and JavaScript changes.
- Removing or changing `ToolbarExtras`.

## Steps
1. Inspect the current toolbar structure and existing extras tests.
2. Add one override parameter with explicit precedence over default toolbar content.
3. Add representative default/override tests and update documentation.
4. Build and run the WrappedToast test suite.

## Verification
- An override is rendered in the existing toolbar location and the normal toolbar is not
  rendered concurrently.
- Without an override, Edit/Save/Cancel and `ToolbarExtras` render as before.
- `dotnet build src/WrappedToast/WrappedToast.csproj` exits 0.
- `dotnet test tests/WrappedToast.Tests/WrappedToast.Tests.csproj` exits 0.

---

Everything above this line is the task specification. Everything below is the
execution record.

# Execution

## Executor Notes
By: <agent/model-or-unknown> @ <YYYY-MM-DDTHH:MM:SS+HH:MM>

## Executor Verification
By: <agent/model-or-unknown> @ <YYYY-MM-DDTHH:MM:SS+HH:MM>

## Reviewer Verification
By: <agent/model-or-unknown> @ <YYYY-MM-DDTHH:MM:SS+HH:MM>

## Review Notes
By: <agent/model-or-unknown> @ <YYYY-MM-DDTHH:MM:SS+HH:MM>
