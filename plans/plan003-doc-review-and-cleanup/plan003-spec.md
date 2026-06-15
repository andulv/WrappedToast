---
type: plan-spec
description: "Plan 003 - Audit and clean up WrappedToast documentation and agent instructions"
status: ready
created: 2026-06-14T19:10:36+02:00
updated: 2026-06-14T20:02:30+02:00
---
# Plan 003 Spec — Documentation Review and Cleanup

## 0. Required Context

- `plan-task-standards` skill (method and templates)
- `README.md` — user-facing project documentation
- `AGENTS.md` — agent entry point
- `.agents/project.instructions.md` — project-specific agent rules
- `.agents/catherder.instructions.md` — CatHerder process rules for this repo
- `.agents/skills/toast-editor-developer/SKILL.md` and its `references/` — generic TOAST UI Editor development skill (framework-neutral, not WrappedToast-specific)

## 1. Goal

Fix factual inaccuracies, reduce redundancy, and fill information gaps in the
WrappedToast documentation suite so that (a) new developers can discover and use
all components from the README alone, and (b) AI agents receive complete,
non-contradictory instructions from `.agents/` files.

## 2. Context / Why

The documentation and instruction files were recently updated. A review against
actual code revealed factual errors (e.g. `<HeadContent>` claim), significant
repetition across three files (API boundary rules stated near-identically in
README, project.instructions, and the skill), and missing information that
would block a new developer (no thin-wrapper parameter table, no `ToastUIEditor`
method list, no install instruction).

## 3. What We Want To Achieve (Outcomes)

- All documentation claims verified against code; no factual errors remain.
- API boundary rules stated once authoritatively (project.instructions.md) and
  referenced (not duplicated) elsewhere.
- README.md self-sufficient for a new developer to use any component, including
  thin wrappers.
- `.agents/` files complete: agents can discover all relevant files, types, and
  conventions without reading source first.
- No significant content duplication across README ↔ project.instructions (skill is now framework-neutral and no longer overlaps).

## 4. Key Principles / Constraints

- README targets human developers; project.instructions targets AI agents.
  The `toast-editor-developer` skill is now framework-neutral TOAST UI guidance
  and does not overlap with WrappedToast-specific rules. Each file owns its
  audience's concerns.
- Changes must not alter the project's API surface or behavior — documentation
  only.
- Existing plan folder structure (`plan001`, `plan002`) must not be disturbed.
- CatHerder plan-task standards apply to this plan itself.

## 5. Out of Scope

- Changing source code, component APIs, or JS interop behavior.
- Renaming or restructuring files outside the documentation suite.
- Editing the `toast-editor-developer` skill or its reference docs (now a generic TOAST UI skill; out of scope for this WrappedToast doc cleanup).
- Editing `CHANGELOG.md`, `THIRD-PARTY-NOTICES.md`, or `LICENSE`.
- Creating or modifying NuGet package metadata.

## 6. Implementation Notes

The user has manually edited README.md (restructured intro, removed "Component
API boundaries" section, split features into per-component sections). Some
findings are now resolved; others remain.

### Already resolved by user edits

| # | Original finding | Resolution |
|---|---|---|
| 9 | API boundary rules duplicated 3× in README | User removed "Component API boundaries" from README. Skill rewritten as generic TOAST UI skill — no longer contains WrappedToast boundary rules. Boundary rules now live only in project.instructions.md. Fully resolved. |

### A. Factual inaccuracies still present (high priority)

| # | Location | Claim | Reality | Fix |
|---|---|---|---|---|
| 1 | README.md:29 | "Components self-register their CSS/JS via `<HeadContent>`" | No `<HeadContent>` usage; `toastui-loader.js` injects `<script>`/`<link>` into `<head>` at runtime | Change to "Components self-register their CSS/JS via the bundled loader — no host-side wiring needed" |
| 2 | README.md:63 | Viewer example includes `["viewer"] = "true"` in Options | Viewer JS already defaults `viewer: true`; redundant in consumer code | Remove `["viewer"] = "true"` from the example |
| 3 | README.md:40 | "Programmatic editor API" listed without specifying which component owns it | These methods are on `WrappedToast`, not the thin wrappers | Clarify: "Programmatic API on WrappedToast: insert, replace, find-and-replace, cursor movement" |

### B. New-developer gaps still present in README.md (medium priority)

| # | Gap | Suggested addition |
|---|---|---|
| 4 | No thin-wrapper parameter table | Add table for `InitialStyle` (`string?`) and `Options` (`Dictionary<string,string>?`) |
| 5 | No `ToastUIEditor` public method summary | Create `doc/api-reference.md` with thin-wrapper method tables; link to it from README |
| 7 | No `HandleSaveAsync` example body | Add a trivial callback implementation |
| 8 | No explanation of how assets load | Mention `toastui-loader.js` auto-injects CSS/JS; no `<script>` tags needed in host |

### C. Redundancy and `.agents/` completeness (low–medium priority)

| # | Issue | Suggested change |
|---|---|---|
| 10 | project.instructions.md "Do not put X" list is verbose | Collapse to single rule + short examples: "No MudBlazor deps, no product transforms, no clipboard/front-matter/host logic in thin wrappers." |
| 11 | project.instructions.md duplicate `WrappedToast`-owns-MudBlazor point (lines 34–35 + 36–37) | Merge into one statement |
| 12 | project.instructions.md missing `FrontMatterPanel` and `TextContentWithFrontMatter` | Add to project shape and mention in implementation guidelines |
| 13 | project.instructions.md missing `toastui-loader.js` | Add to implementation guidelines |
| 14 | project.instructions.md missing `samples/ToastUIEditorAndViewerSamples` in project shape | Add it |
| 15 | project.instructions.md no note that tests are JS-less bUnit | Add brief note next to the test command |
| 16 | No CSS isolation guidance in project.instructions.md | Add note about `.razor.css` files and `::deep` selectors |
| 17 | No note that `EditorOptions`/`ViewerOptions` are mutable shared dictionaries | Add caution in implementation guidelines |

## 7. Open Questions

1. Should the README include a "Getting started / Installation" section with a `dotnet add package` command, given the package is at `0.1.0-preview.1` and may not be on NuGet yet? (resolved: no NuGet section — package is not published yet)
2. Should the `ToastUIEditor` method summary in README be a full table (like `WrappedToast` parameters) or a compact list? (resolved: don't list all methods in README; create a separate `doc/` .md file for the API reference and link to it from README)
3. Is the `<HeadContent>` wording a leftover from a previous implementation that was later changed, or was it always the loader approach? (resolved: code has always used the loader; `<HeadContent>` was never present)
