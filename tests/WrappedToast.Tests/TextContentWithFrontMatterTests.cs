using WrappedToast;

namespace WrappedToast.Tests;

public class TextContentWithFrontMatterTests
{
    [Fact]
    public void Parse_Returns_Empty_FrontMatter_For_Plain_Body()
    {
        var result = TextContentWithFrontMatter.Parse("just a body");

        Assert.NotNull(result);
        Assert.Empty(result!.FrontMatter);
        Assert.Equal("just a body", result.Body);
    }

    [Fact]
    public void Parse_Splits_FrontMatter_And_Body()
    {
        var content = "---\ntitle: Hello\nauthor: tester\n---\nactual body";

        var result = TextContentWithFrontMatter.Parse(content);

        Assert.NotNull(result);
        Assert.Equal("Hello", result!.FrontMatter["title"]);
        Assert.Equal("tester", result.FrontMatter["author"]);
        Assert.Equal("actual body", result.Body);
    }

    [Fact]
    public void Roundtrip_Preserves_FrontMatter_And_Body()
    {
        var content = "---\ntitle: Hello\n---\nbody text";

        var parsed = TextContentWithFrontMatter.Parse(content);
        Assert.NotNull(parsed);

        var roundtripped = parsed!.ToMarkdownWithFrontMatter();

        Assert.Contains("---\ntitle: Hello\n---\n", roundtripped);
        Assert.Contains("body text", roundtripped);
    }

    [Fact]
    public void Parse_With_No_Closing_Delimiter_Treats_Whole_String_As_Body()
    {
        var content = "---\nincomplete";

        var result = TextContentWithFrontMatter.Parse(content);

        Assert.NotNull(result);
        Assert.Empty(result!.FrontMatter);
        Assert.Equal(content, result.Body);
    }
}
