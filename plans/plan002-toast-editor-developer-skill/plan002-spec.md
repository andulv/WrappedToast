---
type: plan-spec
description: "Plan 002 - Add a Toast UI editor developer skill for WrappedToast maintainers"
status: ready
created: 2026-06-14T17:51:55+02:00
updated: 2026-06-14T17:51:55+02:00
---
# Plan 002 Spec — Toast UI Editor Developer Skill

## 0. Required Context

- `plan-task-standards`
- `skill-file-standards`
- `submodules/WrappedToast/.agents/project.instructions.md`
- `submodules/WrappedToast/README.md`
- Upstream TOAST UI docs and source for editor/viewer behavior

## 1. Goal

Create a reusable local skill that helps agents work correctly on WrappedToast
changes involving the native TOAST UI Editor and Viewer JavaScript components.

## 2. Context / Why

WrappedToast intentionally keeps `ToastUIEditor` and `ToastUIEditorViewer` as
thin wrappers around native TOAST UI behavior. Recent confusion came from
assuming native editor and viewer APIs were symmetric when they are not. The
project now requires agents to consult upstream docs/source first, but that
knowledge should also be packaged into a local skill so future work starts from
the right boundary and terminology.

## 3. What We Want To Achieve (Outcomes)

- Add a new skill under `submodules/WrappedToast/.agents/skills/`.
- Keep the skill self-contained, with local reference files derived from the
  official docs and source.
- Capture the important editor/viewer API asymmetries, wrapper-boundary rules,
  and extension areas relevant to WrappedToast maintenance.

## 4. Key Principles / Constraints

- Skill content must be self-contained. External docs are source material, not
  runtime dependencies.
- The skill must reflect WrappedToast's package boundaries:
  thin wrappers vs loader vs higher-level `WrappedToast`.
- Upstream details should be recorded with provenance and snapshot date.
- Keep the skill concise enough to load cheaply, with deeper detail in
  `references/`.

## 5. Out of Scope

- Changing WrappedToast runtime behavior.
- Modifying vendored TOAST UI library files.
- Creating broad generic frontend or Blazor skills not specific to WrappedToast
  and TOAST UI integration work.

## 6. Implementation Notes

- Create a skill named `toast-editor-developer`.
- Put the skill directly under `submodules/WrappedToast/.agents/skills/`.
- Add local reference files for:
  - upstream docs inventory and usage map,
  - native editor/viewer API boundaries,
  - source-derived implementation notes,
  - extension/customization areas.
- Include a concrete workflow for deciding whether a change belongs in native
  wrappers, the loader, or `WrappedToast`.

## 7. Open Questions

1. Where should the skill live? (resolved: `submodules/WrappedToast/.agents/skills/toast-editor-developer`)
2. Should the skill depend on live external docs at runtime? (resolved: no, it must be self-contained and only cite upstream sources in provenance)
3. Should the skill cover only editor/viewer core APIs or also extension areas such as plugins, i18n, widgets, and custom renderers? (resolved: include extension areas because they affect native-component work in WrappedToast)
