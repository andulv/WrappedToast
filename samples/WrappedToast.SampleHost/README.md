# WrappedToast Sample Host

Standalone Blazor Server sample app that demonstrates **WrappedToast** functionality. It proves that the package's static assets are served from `/_content/WrappedToast/...` and that the components work without any host-specific dependencies.

## What the sample exercises

- `<ToastUIEditor>` rendered standalone with a "Read markdown" button that calls `GetMarkdownAsync()` and reports the length.
- `<ToastUIEditorViewer>` rendered standalone with a button that loads a sample markdown body via `SetMarkdown(...)`.
- `<WrappedToast>` round-trip: parses YAML-style front matter into a header table, allows Edit / Save / Cancel, surfaces the saved markdown back through `OnSave`, and renders a host-supplied "Reset sample" button via the `ToolbarExtras` `RenderFragment`.
- Light/dark theme toggle in the app bar — exercises both `toastui-editor.min.css` and `theme/toastui-editor-dark.min.css`.

## Running the sample

```bash
dotnet run --project samples/WrappedToast.SampleHost
```

Then open the URL printed to the console (typically `https://localhost:5001/`).

The sample app depends only on the WrappedToast package, MudBlazor, and ASP.NET Core 10.
