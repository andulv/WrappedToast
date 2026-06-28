---
type: project
description: "WrappedToast project-specific instructions"
alwaysApply: true
---
# Project Instructions - WrappedToast

WrappedToast is a standalone Blazor Razor Class Library wrapping TOAST UI
Editor 3.2.2. It provides two thin Blazor wrappers for the native TOAST UI
components plus one higher-level MudBlazor component built on top of them.

Read first:
- [catherder.project.instructions.md](catherder.project.instructions.md)
- [../README.md](../README.md)

## Project Shape
- `src/WrappedToast` - reusable Razor class library.
  - Public helper types: `FrontMatterPanel`, `TextContentWithFrontMatter`, `FrontMatterRow`.
  - Loader: `wwwroot/toastui-loader.js` — handles lazy-loading of vendored TOAST UI CSS/JS.
  - CSS isolation: components use `.razor.css` files with `::deep` selectors. Preserve this pattern when adding styles.
- `samples/WrappedToast.SampleHost` - Blazor Web App sample host (MudBlazor).
- `samples/ToastUIEditorAndViewerSamples` - plain Blazor sample without MudBlazor.
- `tests/WrappedToast.Tests` - bUnit and unit tests (JS-less; no browser required).
- `plans/` - CatHerder plan artifacts when needed.

## API Boundaries
- `ToastUIEditor` and `ToastUIEditorViewer` are thin wrappers for their TOAST UI
  JavaScript counterparts. They must remain plain Blazor components — no
  MudBlazor dependencies, no product-specific transforms, no clipboard logic,
  no front-matter logic, no host navigation, no app workflow.
- Keep wrapper options and methods aligned with TOAST UI config and API names
  unless .NET naming conventions require a small adaptation.
- `GetHtmlAsync()` on thin wrappers should represent TOAST UI `getHTML()`.
- `WrappedToast` owns higher-level behavior (front matter, toolbar commands,
  copy/export variants, host-facing convenience APIs) and is the only component
  that should take MudBlazor dependencies.
- If raw and transformed HTML are both needed, expose explicit separate
  `WrappedToast` methods, such as `GetHtmlAsync()` and `GetEmailHtmlAsync()`.

## Implementation Guidelines
- Read existing component and JS interop code before changing behavior.
- Before changing behavior involving the native TOAST UI Editor or Viewer
  JavaScript components, read the official API docs and upstream source:
  - Docs: https://nhn.github.io/tui.editor/latest/
  - Source: https://github.com/nhn/tui.editor
  - Do not assume wrapper behavior reflects native component behavior without
    checking upstream docs/source first.
- The bundled `toastui-loader.js` handles lazy-loading of vendored TOAST UI
  CSS/JS. Do not bypass or duplicate this loader.
- `EditorOptions` and `ViewerOptions` on `WrappedToast` are mutable shared
  dictionaries. 
- Keep JavaScript interop modules small and named by responsibility.
- Do not modify vendored TOAST UI files under `src/WrappedToast/wwwroot/lib/`.
- Keep host-specific behavior out of the reusable package unless it is exposed
  as a parameter or render fragment.
- When adding new functionality, decide first which layer it belongs to:
  thin native wrapper, or higher-level `WrappedToast`. Do not blur that split.
- Preserve static web asset paths unless a package identity change requires it.
- Follow the current MudBlazor and Blazor patterns used by the sample and
  components.

## Commands
From this repository root:

- Build package: `dotnet build src/WrappedToast/WrappedToast.csproj`
- Build sample: `dotnet build samples/WrappedToast.SampleHost/WrappedToast.SampleHost.csproj`
- Build plain sample: `dotnet build samples/ToastUIEditorAndViewerSamples/ToastUIEditorAndViewerSamples.csproj`
- Run sample: `dotnet run --project samples/WrappedToast.SampleHost/WrappedToast.SampleHost.csproj`
- Run plain sample: `dotnet run --project samples/ToastUIEditorAndViewerSamples/ToastUIEditorAndViewerSamples.csproj`
- Test (JS-less bUnit): `dotnet test tests/WrappedToast.Tests/WrappedToast.Tests.csproj`
