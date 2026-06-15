---
type: task
description: "Task 003-04 — Add HandleSaveAsync example to README.md"
status: implemented
created: 2026-06-14T19:44:10+02:00
updated: 2026-06-14T20:10:00+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`

## Objective

Add a trivial `HandleSaveAsync` callback example below the WrappedToast usage snippet so new developers see how to handle `OnSave`.

## Scope

- Add a small code block only. No restructuring.

## Steps

1. Below the WrappedToast usage razor snippet (after the `</WrappedToast>` code block and the front-matter explanation paragraph), add:

   ```csharp
   private async Task HandleSaveAsync(string markdown)
   {
       // persist the full markdown (including front matter) here
   }
   ```

## Verification

- README.md contains `HandleSaveAsync` example
- Example is syntactically valid C#

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
