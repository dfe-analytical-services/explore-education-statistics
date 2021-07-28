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
        private readonly StringBuilder _builder = new StringBuilder();

        public string Convert(IElement rootElement)
        {
            _builder.Clear();

            ParseChildren(rootElement, ParseElement);

            var rawLines = _builder.ToString().ToLines();

            // Run post-filtering on lines to remove any extraneous
            // whitespace from the end of lines. This is mostly for
            // better compat with editors/IDEs, where whitespace may
            // be automatically removed upon file save.
            var filteredBuilder = new StringBuilder();

            rawLines.ForEach(line => filteredBuilder.AppendLine(line.TrimEnd()));

            return filteredBuilder.ToString().TrimEnd();
        }

        private void ParseChildren(INode node, Action<IElement> parseChildElement)
        {
            ParseNodes(node.ChildNodes, (child, _) => parseChildElement(child));
        }

        private void ParseNodes(IEnumerable<INode> nodes, Action<IElement, int> parseChildElement)
        {
            nodes.ForEach(
                (node, index) =>
                {
                    switch (node)
                    {
                        case null:
                            return;

                        case IElement element:
                            parseChildElement(element, index);
                            break;

                        case IText text:
                            ParseTextNode(text);
                            break;
                    }
                }
            );
        }

        private void ParseElement(IElement element)
        {
            // If the previous sibling is a non-empty text node, it may
            // not have an associated line break, so we should add one
            // to prevent the current element from being appended
            // on-top of the text node (this looks broken).
            if (element.PreviousSibling?.NodeType == NodeType.Text &&
                element.PreviousSibling?.TextContent?.Trim() != string.Empty)
            {
                _builder.AppendLine();
            }

            switch (element.LocalName)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "p":
                    ParseChildren(element, ParseTextElement);
                    _builder.AppendLine();
                    _builder.AppendLine();
                    break;

                case "ul":
                case "ol":
                    ParseListElement(element);
                    break;

                case "dl":
                    ParseDescriptionListElement(element);
                    break;

                case "blockquote":
                    if (element.Children.Any())
                    {
                        ParseChildren(element, ParseElement);
                    }
                    else
                    {
                        ParseTextElement(element);
                        _builder.AppendLine();
                        _builder.AppendLine();
                    }
                    break;

                case "cite":
                case "figcaption":
                    _builder.Append('—');
                    ParseTextElement(element);
                    _builder.AppendLine();
                    _builder.AppendLine();
                    break;

                case "br":
                    _builder.AppendLine();
                    break;

                case "hr":
                    _builder.AppendLine("---------------");
                    _builder.AppendLine();
                    break;

                case "table":
                    ParseTableElement(element);
                    break;

                default:
                    ParseChildren(element, ParseElement);
                    break;
            }
        }

        private void ParseTextNode(INode node)
        {
            // Text node may be split over multiple lines for
            // formatting purposes in HTML, but this would
            // lead to unnecessary line breaks in rendered text,
            // so we should remove these line breaks.
            var content = node.TextContent.CollapseAndStrip();

            if (content == string.Empty)
            {
                return;
            }

            // As there are potentially inline elements such as span/strong/em,
            // we should try and add an extra space to avoid the text bunching up.
            // We currently only add this if the text content starts with an
            // alphanumeric character, as we can safely assume this isn't a
            // punctuation character. Not sure if there may be edge cases to this?
            if (node.PreviousSibling != null
                && !(node.PreviousSibling is IHtmlBreakRowElement)
                && content[0].IsAlphanumericAscii())
            {
                _builder.Append(' ');
            }

            _builder.Append(content);
        }

        private void ParseTextElement(IElement element)
        {
            switch (element.LocalName)
            {
                case "br":
                    _builder.AppendLine();
                    break;
                default:
                    ParseTextNode(element);
                    break;
            }
        }

        private void ParseListElement(IElement element)
        {
            var isOrdered = element.LocalName == "ol";
            var listItems = element.ChildNodes
                .Where(node => node is IElement { LocalName: "li" })
                .ToList();

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
                                if (lineIndex == 0)
                                {
                                    _builder.AppendLine(line);
                                    return;
                                }

                                _builder.AppendLine(indent + line);
                            }
                        );
                }
            );

            _builder.AppendLine();
        }

        private void ParseDescriptionListElement(IElement element)
        {
            const string indentation = "  ";

            var listItems = element.ChildNodes
                .Where(node => node is IElement { LocalName: "dt" } || node is IElement { LocalName: "dd" });

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

            _builder.AppendLine();
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