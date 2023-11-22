#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils.Html
{
    internal class HtmlToTextConverter
    {
        private readonly StringBuilder _builder = new();

        public string Convert(INodeList nodes)
        {
            _builder.Clear();

            ParseNodes(nodes);

            return Output();
        }

        public string Convert(IElement rootElement)
        {
            _builder.Clear();

            ParseChildren(rootElement);

            return Output();
        }

        private string Output()
        {
            var rawLines = _builder.ToString().ToLines();

            // Run post-filtering on lines to:
            // 1. Remove any extraneous whitespace from the end of lines. This is
            // mostly for better compatibility with formatters / editors / IDEs, where
            // trailing whitespace may be automatically removed upon file save.
            // 2. Ensure we have CRLF (\r\n) line endings for better compatibility with
            // our Windows users (the majority of users). Otherwise, line endings
            // would be server OS dependent and this can lead to inconsistent files.
            var postFilterBuilder = new StringBuilder();

            rawLines.ForEach(line => postFilterBuilder.AppendCrlfLine(line.TrimEnd()));

            return postFilterBuilder.ToString().TrimEnd();
        }

        private void ParseChildren(INode node)
        {
            ParseNodes(node.ChildNodes);
        }

        private void ParseNodes(IEnumerable<INode> nodes)
        {
            ParseNodes(nodes, (child, _) => ParseElement(child));
        }

        private void ParseNodes(IEnumerable<INode> nodes, Action<IElement, int> elementParser)
        {
            nodes.ForEach(
                (node, index) =>
                {
                    switch (node)
                    {
                        case null:
                            return;

                        case IElement element:
                            elementParser(element, index);
                            break;

                        case IText text:
                            ParseEdgeNode(text);
                            break;
                    }
                }
            );
        }

        private void ParseElement(IElement element)
        {
            switch (element.LocalName)
            {
                case "ul":
                case "ol":
                    ParseListElement(element);
                    break;

                case "dl":
                    ParseDescriptionListElement(element);
                    break;

                case "hr":
                    _builder.AppendLine("---------------");
                    break;

                case "table":
                    ParseTableElement(element);
                    break;

                case "a":
                    ParseLinkElement(element);
                    break;

                case "br":
                    _builder.AppendLine();
                    break;

                case "cite":
                    _builder.Append("— ");
                    ParseChildren(element);
                    break;

                default:
                {
                    if (element.HasChildNodes)
                    {
                        ParseChildren(element);
                    }
                    else
                    {
                        ParseEdgeNode(element);
                    }

                    break;
                }
            }

            AddSpacing(element);
        }

        private void AddSpacing(IElement element)
        {
            var next = element.NextNonWhitespaceSibling();

            if (element.IsBlockType() || next is IElement nextElement && nextElement.IsBlockType())
            {
                // Get last 4 chars to check their line endings. Windows uses CRLF line endings (\r\n),
                // so there may be up to 4 chars, instead of 2 for LF (\n) when using Linux / Mac.
                var currentEnding = _builder.Substring(^4..);

                // If already have two line endings, then don't
                // append more spacing as this would be excessive.
                if (currentEnding.EndsWith($"{Environment.NewLine}{Environment.NewLine}"))
                {
                    return;
                }

                // Don't have a trailing line ending currently, so it
                // should be safe to add an additional line ending to
                // provide better spacing with the next block.
                if (!currentEnding.EndsWith(Environment.NewLine))
                {
                    _builder.AppendLine();
                }

                _builder.AppendLine();
            }
        }

        private void ParseEdgeNode(INode node)
        {
            // Text may be split over multiple lines for
            // formatting purposes in HTML, but this would
            // lead to unnecessary line breaks in rendered text,
            // so we should remove these line breaks.
            var content = node.TextContent.CollapseAndStrip();

            if (content == string.Empty)
            {
                return;
            }

            // There are potentially inline elements such as span/strong/em, so we
            // should try add extra spacing to avoid text bunching up. To do this,
            // we need to scan the previous sibling elements of the current node to
            // check that they actually contain something that can be spaced out.
            var prev = node.PreviousNonWhitespaceSibling();

            if (prev is null)
            {
                // If there is no previous non-whitespace sibling, check the siblings of the parent
                // (if it's an element) instead. For example, when starting from the
                // `highlight` text node of `some <strong>highlight</strong>`, we want to be able to
                // locate the `some ` text node that is adjacent to the parent `<strong>` element.
                if (node.Parent is IElement parentElement && parentElement.IsInlineType())
                {
                    prev = parentElement.PreviousNonWhitespaceSibling();
                }
            }

            var hasPreviousInlineSibling =
                prev is IText ||
                (prev is not IHtmlBreakRowElement and IElement prevElement && prevElement.IsInlineType());

            // We currently only add extra spacing if the text content starts with an
            // alphanumeric character as we can safely assume this isn't a
            // punctuation character. Not sure if there may be edge cases to this?
            if (hasPreviousInlineSibling && content[0].IsAlphanumericAscii())
            {
                _builder.Append(' ');
            }

            _builder.Append(content);

            var next = node.NextNonWhitespaceSibling();

            if (next is IElement nextElement && nextElement.IsBlockType())
            {
                _builder.AppendLine();
                _builder.AppendLine();
            }
        }

        private void ParseLinkElement(IElement element)
        {
            ParseChildren(element);

            var href = element.Attributes["href"]?.Value;

            if (!href.IsNullOrWhitespace())
            {
                _builder.Append($" ({href})");
            }
        }

        private void ParseListElement(IElement element)
        {
            var isOrdered = element.LocalName == "ol";
            var listItems = element.ChildNodes
                .Where(node => node is IElement { LocalName: "li" });

            ParseNodes(
                listItems,
                (item, index) =>
                {
                    var lineItemStart = isOrdered ? $"{index + 1}. " : "- ";

                    _builder.Append(lineItemStart);

                    var indent = string.Empty.PadRight(lineItemStart.Length);

                    var converter = new HtmlToTextConverter();
                    var text = converter.Convert(item);

                    text.ToLines()
                        .ForEach(
                            (line, lineIndex) =>
                            {
                                _builder.AppendLine(lineIndex == 0 ? line : indent + line);
                            }
                        );
                }
            );
        }

        private void ParseDescriptionListElement(IElement element)
        {
            const string indentation = "  ";

            var listItems = element.ChildNodes
                .Where(node => node is IElement { LocalName: "dt" } or IElement { LocalName: "dd" });

            ParseNodes(
                listItems,
                (item, _) =>
                {
                    switch (item.LocalName)
                    {
                        case "dt":
                            _builder.AppendLine(item.TextContent.CollapseAndStrip());
                            break;

                        case "dd":
                            var converter = new HtmlToTextConverter();
                            var text = converter.Convert(item);

                            text.ToLines()
                                .ForEach(line => _builder.AppendLine(indentation + line));

                            break;
                    }
                }
            );
        }

        private void ParseTableElement(IElement element)
        {
            var headerRows = element.QuerySelectorAll<IElement>("thead tr");
            var bodyRows = element.QuerySelectorAll<IElement>("tbody tr");

            var table = new HtmlToTextTableRenderer();

            headerRows.ForEach(row => table.AddHeaderRow(row));
            bodyRows.ForEach(row => table.AddBodyRow(row));

            _builder.AppendLine(table.Render());
        }
    }
}
