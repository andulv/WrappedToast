---
type: task
description: "Task 002-01 — Create the toast-editor-developer skill and local upstream references"
status: implemented
created: 2026-06-14T17:51:55+02:00
updated: 2026-06-14T17:54:42+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`
- `skill-file-standards`

Read before starting:
- `submodules/WrappedToast/.agents/project.instructions.md`
- `submodules/WrappedToast/README.md`
- Official TOAST UI docs and source used as input for the skill references

## Objective

Create a self-contained WrappedToast-local skill that guides future agents doing
native TOAST UI editor/viewer integration work.

## Scope

Included:
- Add `submodules/WrappedToast/.agents/skills/toast-editor-developer/`.
- Write `SKILL.md`.
- Add local `references/*.md` files with provenance.
- Capture native API boundaries, source-derived behavior notes, and extension
  areas relevant to WrappedToast.

Excluded:
- Changing runtime package behavior.
- Editing vendored TOAST UI assets.

## Steps

1. Gather the upstream docs and source that matter for WrappedToast work.
2. Distill them into local reference files with provenance and snapshot date.
3. Write a concise `SKILL.md` that tells agents when to use the skill, which
   reference files to read, and how to classify wrapper vs product behavior.
4. Run the smallest practical validation for file structure and references.

## Verification

- `find .agents/skills/toast-editor-developer -maxdepth 2 -type f | sort`
  shows the skill files.
- `rg -n "getHTML|getMarkdown|setMarkdown|isViewer|WrappedToast|loader"`
  `.agents/skills/toast-editor-developer` returns expected matches.
- If available, skill validation scripts exit 0.

---

# Execution

## Executor Notes
By: Codex @ 2026-06-14T17:51:55+02:00

## Executor Verification
By: Codex @ 2026-06-14T17:54:42+02:00

- `find .agents/skills/toast-editor-developer -maxdepth 2 -type f | sort`
  returned the expected skill files.
- `rg -n "getHTML|getMarkdown|setMarkdown|isViewer|WrappedToast|loader|plugin|widgetRules|customHTMLRenderer" .agents/skills/toast-editor-developer -S`
  returned the expected content hits.
- `bash /home/anders/source/agent/catherder-dev/.agents/skills/catherder-skills/skill-file-standards/scripts/validate-references.sh /home/anders/source/agent/catherder-dev/submodules/WrappedToast/.agents/skills/toast-editor-developer`
  exited 0.
- `skills-ref` was not installed, so upstream skill validation could not be
  run.

## Reviewer Verification
By:  @

## Review Notes
By:  @
