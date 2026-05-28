using System.Text;

namespace SampleHost.Components.Pages;

internal sealed record SampleMarkdownDefinition(string Key, string Name, string Description, string Markdown);

internal static class SampleMarkdownContent
{
    internal static readonly IReadOnlyList<SampleMarkdownDefinition> Samples =
    [
        new(
            "basic",
            "Basic Document",
            "Short front matter, heading, list, and code block.",
            """
            ---
            title: Sample document
            author: WrappedToast
            ---

            # WrappedToast Demo

            This is a *sample* markdown document rendered by the **WrappedToast** component.

            - Bullet one
            - Bullet two

            ```csharp
            var x = 1;
            ```
            """),
        new(
            "tables-and-blocks",
            "Tables And Blocks",
            "Moderate sample with tables, callouts, code fences, and nested sections.",
            """
            ---
            title: Product Notes
            category: reference
            tags:
              - sample
              - markdown
              - table
            ---

            # Product Notes

            ## Summary

            This sample mixes several common markdown constructs in one document.

            > Use this to verify spacing, blockquote rendering, and table layout.

            ## Release Checklist

            | Area | Status | Notes |
            | --- | --- | --- |
            | Editor | Done | Keyboard editing works |
            | Viewer | Done | Long content scrolls correctly |
            | Front matter | Done | YAML is preserved |

            ## Example JSON

            ```json
            {
              "title": "WrappedToast",
              "features": ["editor", "viewer", "front matter"],
              "active": true
            }
            ```

            ## Nested Content

            ### Section A

            1. First step
            2. Second step
            3. Third step

            ### Section B

            - Item one
            - Item two
            - Item three
            """),
        new(
            "very-long",
            "Very Long Document",
            "Much taller than a normal browser viewport for scroll testing.",
            BuildVeryLongDocument())
    ];

    internal static string DefaultKey => Samples[0].Key;

    internal static SampleMarkdownDefinition GetByKey(string key)
    {
        return Samples.FirstOrDefault(sample => sample.Key == key) ?? Samples[0];
    }

    private static string BuildVeryLongDocument()
    {
        var builder = new StringBuilder();

        builder.AppendLine("---");
        builder.AppendLine("title: Long Scroll Test");
        builder.AppendLine("category: stress-test");
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine("# Long Scroll Test");
        builder.AppendLine();
        builder.AppendLine("Use this document to verify that the editor or viewer scrolls internally.");
        builder.AppendLine();

        for (var section = 1; section <= 40; section++)
        {
            builder.AppendLine($"## Section {section}");
            builder.AppendLine();
            builder.AppendLine($"This is repeated body content for section {section}. It is intentionally long enough to push the rendered surface far beyond the browser height.");
            builder.AppendLine();
            builder.AppendLine("- First point");
            builder.AppendLine("- Second point");
            builder.AppendLine("- Third point");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine($"section={section}");
            builder.AppendLine("status=scroll-test");
            builder.AppendLine("```");
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
