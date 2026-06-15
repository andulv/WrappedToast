# WrappedToast

- `ToastUIEditor` and `ToastUIEditorViewer`: thin Blazor wrappers around the native TOAST UI JavaScript editor and viewer. They depend on Blazor and the bundled TOAST UI assets only.
- `WrappedToast`: a higher-level MudBlazor component built on top of those thin wrappers.

TOAST UI Editor assets are bundled and automatically loaded by components.


## Projects

- `src/WrappedToast` — reusable Razor class library
- `samples/WrappedToast.SampleHost` — MudBlazor sample application demonstrating all three components in various scenarios
- `samples/ToastUIEditorAndViewerSamples` — plain Blazor sample for `ToastUIEditor` and `ToastUIEditorViewer`. No dependencies except ASP.NET Blazor


## Screenshots

<a href="doc/screenshot-wrappedtoast-light.png">
  <img src="doc/screenshot-wrappedtoast-light.png" alt="WrappedToast light theme" width="480">
</a>

*WrappedToast component in light theme with toolbar and front-matter table.*


<a href="doc/screenshot-editor-viewer-dark.png">
  <img src="doc/screenshot-editor-viewer-dark.png" alt="Editor and viewer dark theme" width="480">
</a>

*ToastUIEditor and ToastUIEditorViewer in dark theme.*


<a href="doc/screenshot-full-height-editor-dark.png">
  <img src="doc/screenshot-full-height-editor-dark.png" alt="Full-height editor dark theme" width="480">
</a>

*Full-height editor layout in dark theme.*


## Dependencies

- TOAST UI Editor 3.2.2 assets are included in this package. vendored under `wwwroot/lib/toastui-editor/` and served at `/_content/WrappedToast/lib/toastui-editor/...` — no CDN dependency
- Components self-register their CSS/JS via the bundled loader. No `<script>` or `<link>` tags needed in the host app.
- WrappedToast and its sample application uses MudBlazor components


## ToastUIEditor / ToastUIEditorViewer - Features

- Thin wrappers around the native TOAST UI Editor and Viewer.
- Constructor options are forwarded through `Options`.
- Straightforward native methods are exposed as Blazor methods. JS callback and DOM-node APIs are intentionally not wrapped as direct C# methods.
- See [ToastUIEditor API Reference](doc/toastuieditor.md) and [ToastUIEditorViewer API Reference](doc/toastuieditorviewer.md) for the full method list.


## WrappedToast - Features

- Optional YAML-style `---` front-matter parsed, displayed, and edited inline
- `ToolbarExtras` `RenderFragment` to inject host-specific buttons without coupling the package to host navigation
- Copy-as-markdown and copy-as-HTML toolbar actions, with clipboard failures surfaced through a `MudBlazor.ISnackbar`
- Programmatic API on WrappedToast: insert, replace, find-and-replace, cursor movement
- Light and dark theme support


## Usage

### Documentation
[Toast UI Editor component API reference](doc/toastuieditor.md) 
[Toast UI Editor Viewer component API reference](doc/toastuieditorviewer.md) 
[WrappedToast component API Reference](doc/wrappedtoast.md) 


### Using ToastUIEditor or ToastUIEditorViewer

```razor
@using WrappedToast

<ToastUIEditor InitialStyle="width:100%;height:400px;"
               Options="@(new Dictionary<string,string> { ["initialEditType"] = "wysiwyg" })" />
```

```razor
@using WrappedToast

<ToastUIEditorViewer InitialStyle="width:100%;height:auto;"
                     Options="@(new Dictionary<string,string> { ["height"] = "auto" })" />
```

### Using WrappedToast

`WrappedToast` is a MudBlazor component. Register MudBlazor in the host app:

```csharp
builder.Services.AddMudServices();
```

Include MudBlazor providers in the host layout:

```razor
<MudThemeProvider />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

```razor
@using WrappedToast

<WrappedToast Title="@FilePath"
              Content="@FileContent"
              OnSave="HandleSaveAsync">
    <ToolbarExtras>
        <MudButton Variant="Variant.Outlined"
                   StartIcon="@Icons.Material.Filled.Print"
                   OnClick="OpenPrintViewAsync">
            Print
        </MudButton>
    </ToolbarExtras>
</WrappedToast>
```

`WrappedToast` parses optional `---` YAML-style front matter from `Content` and displays it as a table above the editor. The full markdown (front matter + body) is delivered back through `OnSave`.

```csharp
private Task HandleSaveAsync(string markdown)
{
    // persist the full markdown (including front matter) here
    return Task.CompletedTask;
}
```

## Samples

### Wrapped Toast, Toast UI Editor and Toast UI Viewer - MudBlazor:

Run the MudBlazor sample host from the repository root:

```bash
dotnet run --project samples/WrappedToast.SampleHost
```

Then open the URL printed to the console (typically `https://localhost:5001/`).

This sample demonstrates all three components, including `WrappedToast` front-matter round-trip, `ToolbarExtras`, and light/dark theme toggling.


### Toast UI Editor and Toast UI Viewer - Plain Blazor:

Run the ToastUIEditorAndViewerSamples sample host from the repository root:

```bash
dotnet run --project samples/ToastUIEditorAndViewerSamples
```

This sample demonstrates the thin `ToastUIEditor` and `ToastUIEditorViewer` wrappers only.


## License and attribution

This package is MIT licensed. It bundles a copy of the TOAST UI Editor (`@toast-ui/editor`), which is also MIT licensed and copyright NHN Cloud Corp. See [`THIRD-PARTY-NOTICES.md`](src/WrappedToast/THIRD-PARTY-NOTICES.md) for the full attribution and license text.
