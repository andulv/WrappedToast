namespace WrappedToast;

/// <summary>
/// Helper that splits a markdown string into optional <c>---</c> YAML-style front matter and a body,
/// and recombines them.
/// </summary>
public class TextContentWithFrontMatter
{
    public static TextContentWithFrontMatter? Parse(string fullContent)
    {
        if (fullContent == null)
        {
            return null;
        }
        var (frontMatter, body) = SplitFrontMatter(fullContent);
        var rows = ParseFrontMatterRows(frontMatter);
        return new TextContentWithFrontMatter
        {
            FrontMatterText = frontMatter,
            FrontMatterRows = rows,
            Body = body,
        };
    }

    /// <summary>
    /// Creates a new instance from separate frontmatter rows and body text.
    /// The <see cref="FrontMatterText"/> is regenerated from the rows.
    /// </summary>
    public static TextContentWithFrontMatter FromParts(IReadOnlyList<FrontMatterRow> rows, string body)
    {
        var frontMatterText = RegenerateFrontMatter(rows);
        return new TextContentWithFrontMatter
        {
            FrontMatterText = string.IsNullOrWhiteSpace(frontMatterText) ? null : frontMatterText,
            FrontMatterRows = rows,
            Body = body,
        };
    }

    /// <summary>
    /// Serializes frontmatter rows back into YAML text (without the <c>---</c> delimiters).
    /// </summary>
    public static string? RegenerateFrontMatter(IReadOnlyList<FrontMatterRow> rows)
    {
        if (rows.Count == 0)
        {
            return null;
        }

        var lines = new List<string>();
        foreach (var row in rows)
        {
            var indent = new string(' ', row.Level * 2);
            if (row.IsSection)
            {
                lines.Add($"{indent}{row.Key}:");
            }
            else
            {
                lines.Add($"{indent}{row.Key}: {row.Value}");
            }
        }

        return string.Join("\n", lines);
    }

    public string? FrontMatterText { get; init; }
    public IReadOnlyList<FrontMatterRow> FrontMatterRows { get; init; } = [];
    public required string Body { get; set; }

    public string ToMarkdownWithFrontMatter()
    {
        if (string.IsNullOrWhiteSpace(FrontMatterText))
        {
            return Body;
        }

        return $"---\n{FrontMatterText.TrimEnd()}\n---\n{Body}";
    }

    private static IReadOnlyList<FrontMatterRow> ParseFrontMatterRows(string? frontMatter)
    {
        if (string.IsNullOrWhiteSpace(frontMatter))
        {
            return [];
        }

        var rows = new List<FrontMatterRow>();
        foreach (var rawLine in frontMatter.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            var trimmed = rawLine.Trim();
            if (trimmed.StartsWith('#') || trimmed == "---")
            {
                continue;
            }

            var indent = rawLine.TakeWhile(char.IsWhiteSpace).Count();
            var level = Math.Clamp(indent / 2, 0, 4);
            var separatorIndex = trimmed.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                rows.Add(new FrontMatterRow(trimmed, string.Empty, level, IsSection: false));
                continue;
            }

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();
            rows.Add(new FrontMatterRow(key, value, level, string.IsNullOrEmpty(value)));
        }

        return rows;
    }

    private static (string? frontMatter, string body) SplitFrontMatter(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return (null, content ?? string.Empty);
        }

        var firstLineEnd = content.IndexOf('\n', StringComparison.Ordinal);
        if (firstLineEnd < 0)
        {
            return (null, content);
        }

        var firstLine = content[..firstLineEnd].TrimEnd('\r');
        if (firstLine != "---")
        {
            return (null, content);
        }

        var frontMatterStart = firstLineEnd + 1;
        var lineStart = frontMatterStart;
        while (lineStart < content.Length)
        {
            var lineEnd = content.IndexOf('\n', lineStart);
            var markerLineEnd = lineEnd < 0 ? content.Length : lineEnd;
            var markerLine = content[lineStart..markerLineEnd].TrimEnd('\r');
            if (markerLine == "---")
            {
                var bodyStart = lineEnd < 0 ? content.Length : lineEnd + 1;
                var frontmatter = content[frontMatterStart..lineStart].TrimEnd('\r', '\n');
                var body = content[bodyStart..];

                return (frontmatter, body);
            }

            if (lineEnd < 0)
            {
                break;
            }

            lineStart = lineEnd + 1;
        }

        return (null, content);
    }
}

/// <summary>
/// Represents a single row in the frontmatter.
/// </summary>
public sealed record FrontMatterRow(string Key, string Value, int Level, bool IsSection);
