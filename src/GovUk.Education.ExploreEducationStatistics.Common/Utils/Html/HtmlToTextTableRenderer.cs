#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils.Html
{
    internal class HtmlToTextTableRenderer
    {
        private const string Separator = "  |  ";
        private const string EmptySeparator = "     ";

        private readonly List<List<TableCell>> _headerRows = new List<List<TableCell>>();
        private readonly List<List<TableCell>> _bodyRows = new List<List<TableCell>>();
        private readonly List<int> _columnWidths = new List<int>();

        private class TableCell
        {
            public readonly IList<string> Lines;

            public readonly int RemainingColSpan;
            public readonly int RemainingRowSpan;

            public TableCell(
                string text,
                int remainingColSpan = 0,
                int remainingRowSpan = 0)
            {
                Lines = text.ToLinesList();
                RemainingColSpan = remainingColSpan;
                RemainingRowSpan = remainingRowSpan;
            }
        }

        public void AddHeaderRow(IElement rowElement)
        {
            _headerRows.Add(
                GenerateTableCells(
                    rowElement: rowElement,
                    previousRowCells: _headerRows.LastOrDefault()
                )
            );
        }

        public void AddBodyRow(IElement rowElement)
        {
            _bodyRows.Add(
                GenerateTableCells(
                    rowElement: rowElement,
                    previousRowCells: _bodyRows.LastOrDefault()
                )
            );
        }

        private List<TableCell> GenerateTableCells(IElement rowElement, List<TableCell>? previousRowCells)
        {
            var cellElements = rowElement.Children
                .Where(cell => cell is IHtmlTableCellElement)
                .Cast<IHtmlTableCellElement>()
                .ToList();

            var cells = new List<TableCell?>();

            if (previousRowCells != null)
            {
                previousRowCells.ForEach(
                    (cell, index) =>
                    {
                        // Insert empty cells from the previous row
                        // that have remaining row span as these need
                        // to take up space in this current row.
                        // We insert nulls to represent cells that are to
                        // be filled by the current list of cell elements.
                        cells.Insert(
                            index,
                            cell.RemainingRowSpan > 0
                                ? new TableCell(
                                    text: string.Empty,
                                    remainingColSpan: cell.RemainingColSpan,
                                    remainingRowSpan: cell.RemainingRowSpan - 1
                                )
                                : null
                        );
                    }
                );
            }
            else
            {
                cellElements.ForEach(
                    (cellElement, index) =>
                    {
                        // Initialise cells with placeholder
                        // nulls so that these can be replaced
                        // by table cells later.
                        cellElement.ColumnSpan
                            .ToEnumerable()
                            .ForEach(
                                span => { cells.Add(null); }
                            );
                    }
                );
            }

            cellElements
                .ForEach(
                    child =>
                    {
                        var converter = new HtmlToTextConverter();
                        var text = converter.Convert(child);

                        var cellIndex = cells.FindIndex(cell => cell == null);

                        var tableCell = new TableCell(
                            text: text,
                            remainingColSpan: child.ColumnSpan - 1,
                            remainingRowSpan: child.RowSpan - 1
                        );

                        cells[cellIndex] = tableCell;

                        // Initialise any missing column widths with 0
                        if (_columnWidths.Count - 1 < cellIndex)
                        {
                            var newColumns = (_columnWidths.Count - 1 - cellIndex)
                                .ToEnumerable()
                                .Select(_ => 0);

                            _columnWidths.AddRange(newColumns);
                        }

                        var childWidth = text.ToLines()
                            .DefaultIfEmpty(string.Empty)
                            .Max(line => line.Length);

                        if (childWidth > _columnWidths[cellIndex])
                        {
                            _columnWidths[cellIndex] = childWidth;
                        }

                        if (child.ColumnSpan == 1)
                        {
                            return;
                        }

                        // We add empty cells with colspans so that we can
                        // create a complete matrix of table cells (instead
                        // of HTML's incomplete representation).
                        var spans = child.ColumnSpan - 1;

                        spans.ToEnumerable()
                            .ForEach(
                                span =>
                                {
                                    cells[cellIndex + span] = new TableCell(
                                        text: string.Empty,
                                        remainingColSpan: spans - span,
                                        remainingRowSpan: tableCell.RemainingRowSpan
                                    );
                                }
                            );
                    }
                );

            return cells as List<TableCell>;
        }

        public string Render()
        {
            var builder = new StringBuilder();

            // Render table header with an extra row that is
            // composed of dashes to separate it from the body.
            if (_headerRows.Count > 0)
            {
                RenderRows(builder, _headerRows);

                _columnWidths.ForEach(
                    (columnWidth, index) =>
                    {
                        builder.Append(string.Empty.PadRight(columnWidth, '-'));

                        if (index != _columnWidths.Count - 1)
                        {
                            builder.Append(Separator);
                        }
                    }
                );

                builder.AppendLine();
            }

            RenderRows(builder, _bodyRows);

            return builder.ToString();
        }

        private void RenderRows(StringBuilder builder, List<List<TableCell>> rows)
        {
            rows.ForEach(
                row =>
                {
                    var lineIndex = 0;
                    var hasLines = true;

                    // As table cells can be multi-line, we need to continue
                    // rendering each row until each cell's lines have been
                    // completely rendered out to the builder.
                    while (hasLines)
                    {
                        var currentLineIndex = lineIndex;

                        row.ForEach(
                            (cell, cellIndex) =>
                            {
                                var columnWidth = _columnWidths[cellIndex];

                                if (cell.Lines.ElementAtOrDefault(currentLineIndex) != null)
                                {
                                    builder.Append(cell.Lines[currentLineIndex].PadRight(columnWidth));
                                }
                                else
                                {
                                    builder.Append(string.Empty.PadRight(columnWidth));
                                }

                                if (cellIndex != row.Count - 1)
                                {
                                    builder.Append(cell.RemainingColSpan == 0 ? Separator : EmptySeparator);
                                }
                            }
                        );


                        hasLines = row.Any(cell => cell.Lines.ElementAtOrDefault(currentLineIndex + 1) != null);
                        lineIndex += 1;

                        builder.AppendLine();
                    }
                }
            );
        }
    }
}