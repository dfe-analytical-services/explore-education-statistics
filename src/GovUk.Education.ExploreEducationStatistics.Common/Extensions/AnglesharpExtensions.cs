#nullable enable
using System.Collections.Generic;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class AnglesharpExtensions
{
    private static readonly IReadOnlySet<string> BlockElements = new HashSet<string>
    {
        "article",
        "aside",
        "blockquote",
        "canvas",
        "dl",
        "dd",
        "dt",
        "fieldset",
        "figcaption",
        "figure",
        "footer",
        "form",
        "header",
        "h1",
        "h2",
        "h3",
        "h4",
        "h5",
        "h6",
        "header",
        "hr",
        "li",
        "main",
        "nav",
        "noscript",
        "ol",
        "p",
        "pre",
        "section",
        "table",
        "tfoot",
        "ul",
        "video"
    };

    private static readonly IReadOnlySet<string> TableElements = new HashSet<string>
    {
        "td",
        "th",
        "tr",
        "thead",
        "tbody"
    };

    public static bool IsBlockType(this IElement element) => BlockElements.Contains(element.LocalName);

    public static bool IsInlineType(this IElement element) => !element.IsBlockType() && !element.IsTableType();

    public static bool IsTableType(this IElement element) => TableElements.Contains(element.LocalName);

    public static bool IsWhitespace(this IText text) => text.TextContent.Trim() == string.Empty;

    public static INode? PreviousNonWhitespaceSibling(this INode node)
    {
        var current = node.PreviousSibling;

        while (current is not null)
        {
            if (current is IElement || current is IText text && !text.IsWhitespace())
            {
                break;
            }

            current = current.PreviousSibling;
        }

        return current;
    }

    public static INode? NextNonWhitespaceSibling(this INode node)
    {
        var current = node.NextSibling;

        while (current is not null)
        {
            if (current is IElement || current is IText text && !text.IsWhitespace())
            {
                break;
            }

            current = current.NextSibling;
        }

        return current;
    }
}
