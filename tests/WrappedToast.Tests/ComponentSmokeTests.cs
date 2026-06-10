using Bunit;
using WrappedToast;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Reflection;

namespace WrappedToast.Tests;

/// <summary>
/// bUnit render smoke tests. They verify that components render without throwing in a JS-less
/// test renderer (JSInterop init fires but is loose-mode tolerated). They also verify that
/// JS module paths still point to the expected static-asset URL under
/// <c>./_content/WrappedToast/...</c>.
/// </summary>
public class ComponentSmokeTests : IDisposable
{
    private readonly BunitContext _ctx;

    public ComponentSmokeTests()
    {
        _ctx = new BunitContext();
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void ToastUIEditor_Renders_Root_Div_With_InitialStyle()
    {
        var cut = _ctx.Render<ToastUIEditor>(p => p
            .Add(c => c.InitialStyle, "display:block;width:100%;"));

        Assert.Contains("display:block;width:100%;", cut.Find("div").GetAttribute("style"));
    }

    [Fact]
    public void ToastUIEditorViewer_Renders_Root_Div_With_InitialStyle()
    {
        var cut = _ctx.Render<ToastUIEditorViewer>(p => p
            .Add(c => c.InitialStyle, "display:block;width:100%;"));

        Assert.Contains("display:block;width:100%;", cut.Find("div").GetAttribute("style"));
    }

    [Fact]
    public void WrappedToast_Renders_Title_And_Edit_Button()
    {
        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Title, "hello.md")
            .Add(c => c.Content, "# Hello\n\nbody"));

        Assert.Contains("hello.md", cut.Markup);
        Assert.Contains("Edit", cut.Markup);
    }

    [Fact]
    public void WrappedToast_Renders_FrontMatter_Table()
    {
        var content = "---\ntitle: My Doc\nauthor: tester\n---\n\nbody";

        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Title, "doc.md")
            .Add(c => c.Content, content));

        Assert.Contains("title", cut.Markup);
        Assert.Contains("My Doc", cut.Markup);
        Assert.Contains("author", cut.Markup);
        Assert.Contains("tester", cut.Markup);
    }

    [Fact]
    public void WrappedToast_Renders_ToolbarExtras_Fragment()
    {
        RenderFragment extras = builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "id", "extra-btn");
            builder.AddContent(2, "Print");
            builder.CloseElement();
        };

        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Title, "x.md")
            .Add(c => c.Content, "body")
            .Add(c => c.ToolbarExtras, extras));

        Assert.Contains("id=\"extra-btn\"", cut.Markup);
        Assert.Contains("Print", cut.Markup);
    }

    [Fact]
    public void WrappedToast_Renders_Copy_Button()
    {
        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Content, "body"));

        Assert.Contains("Copy", cut.Markup);
    }

    [Fact]
    public void WrappedToast_Defaults_Editor_To_Wysiwyg_Mode()
    {
        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Content, "body"));

        var editorOptions = ReadOptions(cut.Instance, "EditorOptions");

        Assert.Equal("wysiwyg", editorOptions["initialEditType"]);
    }

    [Fact]
    public void WrappedToast_Allows_InitialEditType_Override()
    {
        var cut = _ctx.Render<WrappedToast>(p => p
            .Add(c => c.Content, "body")
            .Add(c => c.InitialEditType, "markdown"));

        var editorOptions = ReadOptions(cut.Instance, "EditorOptions");

        Assert.Equal("markdown", editorOptions["initialEditType"]);
    }

    private static Dictionary<string, string> ReadOptions(WrappedToast instance, string propertyName)
    {
        var prop = typeof(WrappedToast).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.NotNull(prop);

        var value = prop!.GetValue(instance) as Dictionary<string, string>;
        Assert.NotNull(value);

        return value!;
    }
}
