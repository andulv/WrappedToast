---
type: task
description: "Task 003-02 — Add thin-wrapper parameter table and asset-loading explanation to README.md"
status: implemented
created: 2026-06-14T19:44:10+02:00
updated: 2026-06-14T20:06:30+02:00
---
## Required Context
Load and follow these skills:
- `plan-task-standards`

## Objective

Add the thin-wrapper parameter table and explain how assets are loaded, so a new developer can use `ToastUIEditor` and `ToastUIEditorViewer` without reading source.

## Scope

- Add a parameter table for `ToastUIEditor` and `ToastUIEditorViewer` (same parameters for both).
- Add a sentence in the Assets section explaining that `toastui-loader.js` auto-injects CSS/JS into the page at runtime — no `<script>` or `<link>` tags needed in the host app.
- Do not add a NuGet/install section (package not published yet).

## Steps

1. In the "ToastUIEditor / ToastUIEditorViewer - Features" section, add a parameter table:

   | Parameter | Type | Default | Description |
   |---|---|---|---|
   | `InitialStyle` | `string?` | `null` | Inline `style` attribute on the root `<div>` |
   | `Options` | `Dictionary<string,string>?` | `null` | Options forwarded to the TOAST UI constructor |

2. In the Assets section, add after the existing bullet: "The bundled `toastui-loader.js` auto-injects the CSS and JS into the page when a component initializes — no `<script>` or `<link>` tags needed in the host app."

## Verification

- README.md contains the thin-wrapper parameter table with `InitialStyle` and `Options`
- README.md mentions `toastui-loader.js` and states no `<script>`/`<link>` tags are needed

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
