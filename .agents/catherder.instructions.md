---
description: "CatHerder always-follow process rules - must be read by any agents working in this repository."
applyTo: "**"
---
# CatHerder Process Rules
These rules apply to all work in this project.

Repository-specific guidance is in [project.instructions.md](project.instructions.md).

## Plans and tasks - Intentions vs. execution
Plans define intent. Tasks execute intent. Do not merge these concerns.

## Scope Boundary
Scope is this project only.

- Treat files outside this repository as read-only by default.
- Do not edit outside this repository unless the user explicitly requests it.
- If multiple projects are open, confirm the active project root first.

## Planning vs Execution
Planning mode means creating or updating plan files, breaking goals into tasks,
or resolving open questions. Do not start implementation work that belongs in
tasks while planning.

Execution mode means an implementation plan is active and you are executing a
specific task. Execute one task at a time. Do not silently change plan scope
while executing a task.

If missing requirements appear during execution, stop and ask whether to return
to planning or continue as-is.

## Plans
Plans live under [plans/](../plans/) when this repository needs durable
CatHerder planning artifacts.

Use the surrounding CatHerder standards when available. Keep plan files concise,
with falsifiable verification steps and clear acceptance criteria.

## Execution Loop
1. Understand instructions and current code.
2. Plan the change when the work is non-trivial.
3. Execute one task or coherent change at a time.
4. Verify with the smallest relevant build, test, or manual check.
5. Report outcome, verification, and remaining risks.

## Prompt Precedence
When multiple instruction sources conflict, follow this order:

1. System instructions.
2. Developer instructions.
3. User instructions.
4. Project process rules under `.agents/`.
5. Active plan under `plans/`.
6. Task files referenced by the plan.
7. Other project docs and code comments.
8. Host-injected metadata.

## Persistence of decisions
Prefer durable artifacts over chat-only decisions:

- Write architecture or API decisions into README, plan notes, or project docs.
- Store supporting material under the relevant plan folder.
- Update timestamps if a plan/task format requires them.

## Stop Rules
Stop and ask if:

- Requirements are ambiguous.
- A task conflicts with the plan or project instructions.
- Architectural decisions are needed that the plan does not cover.
- Verification is not possible.
- Required context is missing.
- Two high-precedence instruction sources conflict.

## Timestamps
Use full ISO 8601 with seconds and timezone offset:
`YYYY-MM-DDTHH:MM:SS+HH:MM`.
