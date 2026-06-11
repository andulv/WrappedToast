# WrappedToast

A standalone Blazor Razor Class Library that wraps [TOAST UI Editor](https://github.com/nhn/tui.editor) v3.2.2. Provides `ToastUIEditor`, `ToastUIEditorViewer`, and a higher-level `WrappedToast` component with a MudBlazor view/edit toolbar and YAML front-matter support. TOAST UI Editor assets are bundled; no CDN required.

## Projects

- `src/WrappedToast` — reusable Razor class library
- `samples/WrappedToast.SampleHost` — Blazor Web App sample demonstrating all three components

## Features

- **`ToastUIEditor`** — full TOAST UI Editor surface (WYSIWYG and markdown modes)
- **`ToastUIEditorViewer`** — read-only TOAST UI markdown viewer
- **`WrappedToast`** — higher-level component combining editor/viewer with a MudBlazor toolbar (Edit / Save / Cancel) and optional YAML front-matter table
- TOAST UI Editor 3.2.2 assets vendored under `wwwroot/lib/toastui-editor/` and served at `/_content/WrappedToast/lib/toastui-editor/...` — no CDN dependency
- Components self-register their CSS/JS via `<HeadContent>` — no host-side wiring needed
- Optional YAML-style `---` front-matter parsed, displayed, and edited inline
- `ToolbarExtras` `RenderFragment` to inject host-specific buttons without coupling the package to host navigation
- Copy-as-markdown and copy-as-HTML toolbar actions
- Programmatic editor API: insert, replace, find-and-replace, cursor movement
- Light and dark theme support

## Usage

Register MudBlazor in the host app:

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

### ToastUIEditor

```razor
@using WrappedToast

<ToastUIEditor InitialStyle="width:100%;height:400px;"
               Options="@(new Dictionary<string,string> { ["initialEditType"] = "wysiwyg" })" />
```

### ToastUIEditorViewer

```razor
@using WrappedToast

<ToastUIEditorViewer InitialStyle="width:100%;height:auto;"
                     Options="@(new Dictionary<string,string> { ["viewer"] = "true", ["height"] = "auto" })" />
```

### WrappedToast

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

`WrappedToast` parses optional `---` YAML-style front matter from `Content` and displays it as a read-only table above the editor. The full markdown (front matter + body) is delivered back through `OnSave`.

## WrappedToast Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Optional title rendered in the toolbar |
| `ShowTitle` | `bool` | `true` | Whether to show `Title` in the toolbar |
| `Content` | `string` | `""` | Full markdown content, optionally including `---` YAML front matter |
| `OnSave` | `EventCallback<string>` | — | Invoked on save with the full markdown including front matter |
| `InitialEditType` | `string` | `"wysiwyg"` | Initial TOAST UI editor mode (`"wysiwyg"` or `"markdown"`) |
| `ToolbarExtras` | `RenderFragment?` | `null` | Render fragment placed next to the Edit button (view mode only) |
| `ViewerLinkBaseHref` | `string?` | `null` | Base href for resolving relative markdown links in the viewer |
| `ViewerImageBaseHref` | `string?` | `null` | Base href for resolving relative markdown image sources in the viewer |

## Sample

Run the sample from the repository root:

```bash
dotnet run --project samples/WrappedToast.SampleHost
```

Then open the URL printed to the console (typically `https://localhost:5001/`).

The sample demonstrates `ToastUIEditor`, `ToastUIEditorViewer`, and `WrappedToast` including round-trip front-matter editing, the `ToolbarExtras` extension point, and light/dark theme toggling.

## License and attribution

This package is MIT licensed. It bundles a copy of the TOAST UI Editor (`@toast-ui/editor`), which is also MIT licensed and copyright NHN Cloud Corp. See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for the full attribution and license text.

## Versioning

See [`CHANGELOG.md`](CHANGELOG.md).
