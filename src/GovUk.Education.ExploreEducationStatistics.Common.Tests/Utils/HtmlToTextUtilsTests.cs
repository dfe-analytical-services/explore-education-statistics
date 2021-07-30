#nullable enable
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public class HtmlToTextUtilsTests
    {
        private readonly string _dir;

        public HtmlToTextUtilsTests()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (directoryName != null)
            {
                _dir = directoryName;
            }
            else
            {
                throw new Exception("Could not locate tests directory");
            }
        }

        [Fact]
        public async Task HtmlToText_EmptyString()
        {
            Assert.Empty(await HtmlToTextUtils.HtmlToText(""));
        }

        [Fact]
        public async Task HtmlToText_WhitespaceStrings()
        {
            Assert.Empty(await HtmlToTextUtils.HtmlToText("  "));
            Assert.Empty(await HtmlToTextUtils.HtmlToText("  \n  "));
            Assert.Empty(await HtmlToTextUtils.HtmlToText("  \r\n  "));
        }

        [Fact]
        public async Task HtmlToText_SingleElement()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                "<p>Test paragraph</p>");

            Assert.Equal("Test paragraph", text);
        }

        [Fact]
        public async Task HtmlToText_SingleElementInDiv()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                "<div><p>Test paragraph</p></div>");

            Assert.Equal("Test paragraph", text);
        }

        [Fact]
        public async Task HtmlToText_InlineElements()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                "<p>Test paragraph with <strong>bold text</strong> and <em>italic text</em></p>");

            Assert.Equal("Test paragraph with bold text and italic text", text);
        }

        [Fact]
        public async Task HtmlToText_InlineElementsWithMultilineFormatting()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"<p>
                    Test paragraph with 
                    <strong>bold text</strong> 
                    and <em>italic text</em> and
                    <small>small text</small>
                  </p>");

            Assert.Equal("Test paragraph with bold text and italic text and small text", text);
        }

        [Fact]
        public async Task HtmlToText_InlineElementsWithPunctuation()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"<p>
                    Test paragraph with 
                    <strong>bold text</strong>, 
                    <em>italic text</em>! And
                    <small>small text</small>.
                    <strong>Next sentence?</strong>
                    Over here.
                  </p>");

            Assert.Equal("Test paragraph with bold text, italic text! And small text. Next sentence? Over here.", text);
        }

        [Fact]
        public async Task HtmlToText_MultipleElements()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <h1>Test heading 1</h1>
                <h2>Test heading 2</h2>
                <h3>Test heading 3</h3>
                <h4>Test heading 4</h4>
                <h5>Test heading 5</h5>
                <h6>Test heading 6</h6>
                <p>Test paragraph 1</p>
                <span>Test span 1</span>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_MultipleElementsInDiv()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <div>
                    <h1>Test heading 1</h1>
                    <h2>Test heading 2</h2>
                    <h3>Test heading 3</h3>
                    <h4>Test heading 4</h4>
                    <h5>Test heading 5</h5>
                    <h6>Test heading 6</h6>
                    <p>Test paragraph 1</p>
                    <span>Test span 1</span>
                </div>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_HorizontalLineElement()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <p>Test paragraph 1</p>
                <hr/>
                <p>Test paragraph 2</p>
                ");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_LineBreakElements()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <p>Test paragraph 1
                    <br/>with line break</p>
                <br/>
                <p>Test paragraph 2</p>
                ");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_UnorderedList()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ul>
                    <li>List item 1</li>
                    <li>List item 2</li>
                </ul>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_UnorderedList_HasLineAfter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ul>
                    <li>List item 1</li>
                    <li>List item 2</li>
                </ul>
                <p>Paragraph after</p>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_OrderedList()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ol>
                    <li>List item 1</li>
                    <li>List item 2</li>
                </ol>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_OrderedList_HasLineAfter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ol>
                    <li>List item 1</li>
                    <li>List item 2</li>
                </ol>
                <p>Paragraph after</p>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_OrderedList_OverTenItemsWithMultiline()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ol>
                    <li>List item 1</li>
                    <li>List item 2</li>
                    <li>List item 3</li>
                    <li>List item 4</li>
                    <li>List item 5</li>
                    <li>List item 6</li>
                    <li>List item 7</li>
                    <li>List item 8</li>
                    <li>
                        <p>List item 9</p>
                        <p>Over multiple lines</p>
                    </li>
                    <li>List item 10</li>
                    <li>
                        <p>List item 11</p>
                        <p>Over multiple lines</p>
                    </li>
                </ol>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_UnorderedList_WithNestedText()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ul>
                    <li>List item 1</li>
                    <li>
                        List item 2
                        <p>List item 2 paragraph 1</p>
                        <p>List item 2 paragraph 2</p>
                    </li>
                    <li>List item 3</li>
                </ul>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_UnorderedList_WithNestedList()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ul>
                    <li>List item 1</li>
                    <li>
                        Nested list
                        <ul>
                            <li>Nested list item 1</li>
                            <li>Nested list item 2</li>
                        </ul>
                    </li>
                </ul>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_UnorderedList_WithDeeplyNestedList()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ul>
                    <li>List item 1</li>
                    <li>
                        Nested list
                        <ul>
                            <li>Nested 1 list item 1</li>
                            <li>
                                Nested 1 list item 2 
                                <ul>
                                    <li>Nested 2 list item 1</li>
                                    <li>Nested 2 list item 2</li>
                                </ul>
                            </li>
                        </ul>
                    </li>
                </ul>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_UnorderedList_OverTenItemsWithMultiline()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <ul>
                    <li>List item 1</li>
                    <li>List item 2</li>
                    <li>List item 3</li>
                    <li>List item 4</li>
                    <li>List item 5</li>
                    <li>List item 6</li>
                    <li>List item 7</li>
                    <li>List item 8</li>
                    <li>
                        <p>List item 9</p>
                        <p>Over multiple lines</p>
                    </li>
                    <li>List item 10</li>
                    <li>
                        <p>List item 11</p>
                        <p>Over multiple lines</p>
                    </li>
                </ul>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_DescriptionList()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <dl>
                    <dt>Term 1</dt>
                    <dd>Description 1</dd>
                    <dt>Term 2</dt>
                    <dd>Description 2</dd>
                    <dt>Term 3</dt>
                    <dd>Description 3</dd>
                </dl>
                <p>Paragraph after</p>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_DescriptionList_HasLineAfter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <dl>
                    <dt>Term 1</dt>
                    <dd>Description 1</dd>
                    <dt>Term 2</dt>
                    <dd>Description 2</dd>
                    <dt>Term 3</dt>
                    <dd>Description 3</dd>
                </dl>
                <p>Paragraph after</p>");

            Snapshot.Match(text);
        }


        [Fact]
        public async Task HtmlToText_DescriptionList_WithMultilineItem()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <dl>
                    <dt>Term 1</dt>
                    <dd>Description 1</dd>
                    <dt>Term 2</dt>
                    <dd>
                        Description 2
                        <p>Description 2 paragraph 1</p>
                        <ul>
                            <li>Description 2 list item 1</li>
                            <li>Description 2 list item 2</li>
                        </ul>
                    </dd>
                    <dt>Term 3</dt>
                    <dd>Description 3</dd>
                </dl>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_Blockquote()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                "<blockquote>Test quote</blockquote>");

            Assert.Equal("Test quote", text);
        }

        [Fact]
        public async Task HtmlToText_Blockquote_HasLineAfter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <blockquote>Test quote</blockquote>
                <p>Paragraph after</p>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_Blockquote_WithParagraphs()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <blockquote>
                    <p>Test paragraph quote 1</p>
                    <p>Test paragraph quote 2</p>
                </blockquote>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_Blockquote_WithCaption()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <figure>
                    <blockquote>
                        <p>Test paragraph quote 1</p>
                        <p>Test paragraph quote 2</p>
                    </blockquote>
                    <figcaption>
                        <cite>Test citation</cite>
                    </figcaption>
                </figure>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_Blockquote_WithCaptionHasLineAfter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <figure>
                    <blockquote>
                        <p>Test paragraph quote 1</p>
                        <p>Test paragraph quote 2</p>
                    </blockquote>
                    <figcaption>
                        <cite>Test citation</cite>
                    </figcaption>
                </figure>
                <p>Paragraph after</p>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_PadsToLargestCell()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                        </tr>
                        <tr>
                            <td>Cell 3 with more text</td>
                            <td>Cell 4</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_MultilineCell()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td>
                                <p>Cell 5 paragraph 1</p>
                                <ul>
                                    <li>Cell 5 list item 1</li>
                                    <li>Cell 5 list item 2</li>
                                <ul>
                            </td>
                            <td>Cell 6</td>
                        </tr>
                        <tr>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                            <td>Cell 9</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_EmptyCells()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td></td>
                            <td>Cell 2</td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>Cell 4</td>
                            <td>Cell 5</td>
                        </tr>
                        <tr>
                            <td>Cell 6</td>
                            <td>Cell 7</td>
                            <td></td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowHeader()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <thead>
                        <tr>
                            <th>Header 1</th>
                            <th>Header 2</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                        </tr>
                        <tr>
                            <td>Cell 3</td>
                            <td>Cell 4</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_ColumnHeader()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <th>Header 1</th>
                            <th>Header 1</th>
                        </tr>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_ColumnAndRowHeaders()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <thead>
                        <tr>
                            <th>Header 1</th>
                            <th>Header 2</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <th>Header 3</th>
                            <td>Cell 1</td>
                        </tr>
                        <tr>
                            <th>Header 4</th>
                            <td>Cell 2</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_ColSpans()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td colspan=""2"">Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td colspan=""3"">Cell 5</td>
                        </tr>
                        <tr>
                            <td colspan=""2"">Cell 6</td>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_ColSpanWithMultiline()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td colspan=""2"">Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td colspan=""3"">
                                <p>Cell 5 paragraph 1</p>
                                <ul>
                                    <li>Cell 5 list item 1</li>
                                    <li>Cell 5 list item 2</li>
                                <ul>                    
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""2"">Cell 6</td>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowHeaderWithColSpans()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <thead>
                        <tr>
                            <th>Header 1</th>
                            <th colspan=""2"">Header 2</th>
                        </tr>
                        <tr>
                            <th>Header 3</th>
                            <th>Header 4</th>
                            <th>Header 5</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td>Cell 5</td>
                            <td>Cell 6</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_ColumnHeaderWithColSpans()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <th colspan=""2"">Header 1</th>
                            <td>Cell 1</td>
                        </tr>
                        <tr>
                            <th>Header 2</th>
                            <th>Header 3</th>
                            <td>Cell 2</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowSpans()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td rowspan=""3"">Cell 1</td>
                            <td>Cell 2</td>
                            <td rowspan=""2"">Cell 3</td>
                        </tr>
                        <tr>
                            <td rowspan=""2"">Cell 4</td>
                        </tr>
                        <tr>
                            <td>Cell 5</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowSpansWithMultiline()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td rowspan=""3"">Cell 1</td>
                            <td>Cell 2</td>
                            <td rowspan=""2"">Cell 3</td>
                        </tr>
                        <tr>
                            <td rowspan=""2"">
                                <p>Cell 4 paragraph 1</p>
                                <ul>
                                    <li>Cell 4 list item 1</li>
                                    <li>Cell 4 list item 2</li>
                                <ul>
                            </td>
                        </tr>
                        <tr>
                            <td>Cell 5</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowHeaderWithRowSpans()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <thead>
                        <tr>
                            <th rowspan=""2"">Header 1</th>
                            <th>Header 2</th>
                            <th>Header 3</th>
                        </tr>
                        <tr>
                            <th>Header 4</th>
                            <th>Header 5</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td>Cell 5</td>
                            <td>Cell 6</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_ColHeaderWithRowSpans()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <th rowspan=""2"">Header 1</th>
                            <th>Header 2</th>
                            <td>Cell 1</td>
                        </tr>
                        <tr>
                            <th>Header 3</th>
                            <td>Cell 2</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInTopLeft()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td colspan=""2"" rowspan=""2"">Cell 1</td>
                            <td>Cell 2</td>
                        </tr>
                        <tr>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td>Cell 5</td>
                            <td>Cell 6</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInTopCenter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td colspan=""2"" rowspan=""2"">Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 3</td>
                            <td>Cell 4</td>
                        </tr>
                        <tr>
                            <td>Cell 5</td>
                            <td>Cell 6</td>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInTopRight()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td colspan=""2"" rowspan=""2"">Cell 2</td>
                        </tr>
                        <tr>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td>Cell 5</td>
                            <td>Cell 6</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInCenterLeft()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td colspan=""2"" rowspan=""2"">Cell 4</td>
                            <td>Cell 5</td>
                        </tr>
                        <tr>
                            <td>Cell 6</td>
                        </tr>
                        <tr>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                            <td>Cell 9</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInCenterRight()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td colspan=""2"" rowspan=""2"">Cell 5</td>
                        </tr>
                        <tr>
                            <td>Cell 6</td>
                        </tr>
                        <tr>
                            <td>Cell 6</td>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInBottomLeft()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td colspan=""2"" rowspan=""2"">Cell 4</td>
                            <td>Cell 5</td>
                        </tr>
                        <tr>
                            <td>Cell 6</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInBottomCenter()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                            <td>Cell 7</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td colspan=""2"" rowspan=""2"">Cell 5</td>
                            <td>Cell 6</td>
                        </tr>
                        <tr>
                            <td>Cell 7</td>
                            <td>Cell 8</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TableElement_RowAndColSpansInBottomRight()
        {
            var text = await HtmlToTextUtils.HtmlToText(
                @"
                <table>
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                            <td>Cell 3</td>
                        </tr>
                        <tr>
                            <td>Cell 4</td>
                            <td colspan=""2"" rowspan=""2"">Cell 5</td>
                        </tr>
                        <tr>
                            <td>Cell 6</td>
                        </tr>
                    </tbody>
                </table>");

            Snapshot.Match(text);
        }

        [Fact]
        public async Task HtmlToText_TestHtml1()
        {
            var html = await File.ReadAllTextAsync(Path.Combine(_dir, "Resources/test-html-1.html"));
            var text = await HtmlToTextUtils.HtmlToText(html);

            Snapshot.Match(text);
        }
    }
}