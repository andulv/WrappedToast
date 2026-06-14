using System.Reflection;
using WrappedToast;

namespace WrappedToast.Tests;

public class ThinWrapperApiSurfaceTests
{
    [Fact]
    public void ToastUIEditor_Exposes_Direct_Native_PassThrough_Methods()
    {
        var methodNames = GetPublicMethodNames(typeof(ToastUIEditor));

        string[] expected =
        [
            "BlurAsync",
            "ChangeModeAsync",
            "ChangePreviewStyleAsync",
            "ConvertPositionToMatchEditorModeAsync",
            "DeleteSelectionAsync",
            "DestroyAsync",
            "ExecAsync",
            "FocusAsync",
            "GetCurrentPreviewStyleAsync",
            "GetHeightAsync",
            "GetHtmlAsync",
            "GetMarkdownAsync",
            "GetMinHeightAsync",
            "GetRangeInfoOfNodeAsync",
            "GetScrollTopAsync",
            "GetSelectedTextAsync",
            "GetSelectionAsync",
            "HideAsync",
            "InsertTextAsync",
            "InsertToolbarItemAsync",
            "IsMarkdownModeAsync",
            "IsViewerAsync",
            "IsWysiwygModeAsync",
            "MoveCursorToEndAsync",
            "MoveCursorToStartAsync",
            "OffAsync",
            "RemoveHookAsync",
            "RemoveToolbarItemAsync",
            "ReplaceSelectionAsync",
            "ReplaceWithWidgetAsync",
            "ResetAsync",
            "SetHeightAsync",
            "SetHtmlAsync",
            "SetMarkdown",
            "SetMarkdownAsync",
            "SetMinHeightAsync",
            "SetPlaceholderAsync",
            "SetScrollTopAsync",
            "SetSelectionAsync",
            "ShowAsync"
        ];

        foreach (var name in expected)
        {
            Assert.Contains(name, methodNames);
        }
    }

    [Fact]
    public void ToastUIEditorViewer_Exposes_Documented_Direct_Methods()
    {
        var methodNames = GetPublicMethodNames(typeof(ToastUIEditorViewer));

        string[] expected =
        [
            "DestroyAsync",
            "IsMarkdownModeAsync",
            "IsViewerAsync",
            "IsWysiwygModeAsync",
            "OffAsync",
            "SetMarkdown",
            "SetMarkdownAsync"
        ];

        foreach (var name in expected)
        {
            Assert.Contains(name, methodNames);
        }
    }

    private static HashSet<string> GetPublicMethodNames(Type type)
    {
        return type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);
    }
}
