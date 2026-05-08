using System.Reflection;
using WrappedToast;

namespace WrappedToast.Tests;

/// <summary>
/// Verifies that the JS module path used at runtime points at the package's static-web-assets URL,
/// so consumers don't need to re-route the JS module from a different location.
/// </summary>
public class JsModulePathTests
{
    [Fact]
    public void ToastUIEditor_JsModulePath_Points_At_Package_Static_Asset()
    {
        var path = ReadModulePath(new ToastUIEditor());
        Assert.Equal("./_content/WrappedToast/ToastUIEditor.razor.js", path);
    }

    [Fact]
    public void ToastUIEditorViewer_JsModulePath_Points_At_Package_Static_Asset()
    {
        var path = ReadModulePath(new ToastUIEditorViewer());
        Assert.Equal("./_content/WrappedToast/ToastUIEditorViewer.razor.js", path);
    }

    private static string ReadModulePath(ToastUIEditorCore instance)
    {
        var prop = typeof(ToastUIEditorCore).GetProperty(
            "JsModulePath",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(prop);
        var value = (string?)prop!.GetValue(instance);
        Assert.NotNull(value);
        return value!;
    }
}
