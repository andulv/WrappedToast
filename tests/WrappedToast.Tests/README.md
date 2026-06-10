# WrappedToast Tests

Unit tests for the WrappedToast Blazor components. Run with:

```bash
dotnet test
```

Tests are implemented with [bUnit](https://bunit.dev/) and [xUnit](https://xunit.net/).
                     InitialStyle="display:block;width:100%;height:auto;"
                     Options="@(new Dictionary<string,string>{ ["viewer"]="true", ["height"]="auto" })" />
```

### WrappedToast

A higher-level component that combines viewer/editor toggling with an Edit / Save / Cancel toolbar (MudBlazor based). Supply markdown via `Content` and respond to `OnSave`. Use the optional `ToolbarExtras` `RenderFragment` to inject extra buttons (e.g. a print or share button) without coupling the package to host-specific navigation.

```razor
<WrappedToast Title="@FilePath"
              Content="@FileContent"
              OnSave="HandleSaveAsync">
    <ToolbarExtras>
        <MudButton Variant="Variant.Outlined" StartIcon="@Icons.Material.Filled.Print" OnClick="OpenPrintViewAsync">
            Print
        </MudButton>
    </ToolbarExtras>
</WrappedToast>
```

`WrappedToast` parses optional `---` YAML-style front matter from `Content` and shows it as a small read-only table above the editor. The full markdown (front matter + body) is delivered back through `OnSave`.

## Bundled assets

TOAST UI Editor assets are vendored at `wwwroot/lib/toastui-editor/` and served at `/_content/CatHerder.Components.ToastEditor/lib/toastui-editor/...`.

The components inject the required `<link>` and `<script>` tags themselves via `<HeadContent>` (per-page) — no host-side wiring is required.

## License and attribution

This package is MIT licensed. It bundles a copy of the TOAST UI Editor (`@toast-ui/editor`), which is also MIT licensed and copyright NHN Cloud Corp. See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for the full attribution and license text.

## Versioning

See [`CHANGELOG.md`](CHANGELOG.md).
