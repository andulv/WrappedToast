---
type: task
description: "Task 003-01 — Fix three factual errors in README.md"
status: implemented
created: 2026-06-14T19:44:10+02:00
updated: 2026-06-14T20:06:30+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`

## Objective

Fix the three factual inaccuracies currently in README.md.

## Scope

Three targeted edits only — no restructuring or new content.

## Steps

1. Line 29: change "Components self-register their CSS/JS via `<HeadContent>` — no host-side wiring needed" to "Components self-register their CSS/JS via the bundled loader — no host-side wiring needed"
2. Line 63: remove `["viewer"] = "true"` from the `ToastUIEditorViewer` example Options dictionary (keep `["height"] = "auto"` only)
3. Line 40: change "Programmatic editor API: insert, replace, find-and-replace, cursor movement" to "Programmatic API on WrappedToast: insert, replace, find-and-replace, cursor movement"

## Verification

- README.md contains "bundled loader" and does not contain `<HeadContent>`
- README.md viewer example does not contain `["viewer"]`
- README.md contains "Programmatic API on WrappedToast"

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
