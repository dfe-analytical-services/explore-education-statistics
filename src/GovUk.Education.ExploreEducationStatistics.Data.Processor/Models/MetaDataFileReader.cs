#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

/// <summary>
/// Class responsible for up-front calculation of the column indexes to look up particular
/// pieces of information from a given metadata file, and for reading of Subject metadata
/// from the file.
/// </summary>
public class MetaDataFileReader
{
    private readonly Dictionary<MetaColumns, int> _metaColumnIndexes;

    public MetaDataFileReader(List<string> metaCsvHeaders)
    {
        _metaColumnIndexes = EnumUtil.GetEnums<MetaColumns>()
            .ToDictionary(
                column => column,
                column => metaCsvHeaders.FindIndex(h => h.Equals(column.ToString()))
            );
    }

    public List<MetaRow> GetMetaRows(List<List<string>> metaRowValues) =>
        metaRowValues.Select(GetMetaRow).ToList();

    public MetaRow GetMetaRow(IReadOnlyList<string> rowValues)
    {
        var columnType = ReadMetaColumnValue(MetaColumns.col_type, rowValues);
        var indicatorUnit = ReadMetaColumnValue(MetaColumns.indicator_unit, rowValues);
        var indicatorDp = ReadMetaColumnValue(MetaColumns.indicator_dp, rowValues);

        return new MetaRow
        {
            ColumnName = ReadMetaColumnValue(MetaColumns.col_name, rowValues),
            ColumnType = Enum.Parse<ColumnType>(columnType!),
            Label = ReadMetaColumnValue(MetaColumns.label, rowValues),
            FilterGroupingColumn = ReadMetaColumnValue(MetaColumns.filter_grouping_column, rowValues),
            ParentFilter = ReadMetaColumnValue(MetaColumns.parent_filter, rowValues),
            FilterHint = ReadMetaColumnValue(MetaColumns.filter_hint, rowValues),
            AutoSelectFilterItemLabel = ReadMetaColumnValue(MetaColumns.filter_default, rowValues),
            IndicatorGrouping = ReadMetaColumnValue(MetaColumns.indicator_grouping, rowValues),
            IndicatorUnit = EnumUtil.GetFromEnumValue<IndicatorUnit>(indicatorUnit.DefaultsTo("")),
            DecimalPlaces = !indicatorDp.IsNullOrEmpty() ? int.Parse(indicatorDp!) : null
        };
    }

    public List<(Filter Filter, string Column)> ReadFiltersFromCsv(
        IEnumerable<MetaRow> metaRows,
        Subject subject)
    {
        return metaRows
            .Where(row => row.ColumnType == ColumnType.Filter)
            .Select(filterMetaRow => (
                filter: new Filter(
                    hint: filterMetaRow.FilterHint,
                    label: filterMetaRow.Label,
                    name: filterMetaRow.ColumnName,
                    groupCsvColumn: filterMetaRow.FilterGroupingColumn,
                    parentFilter: filterMetaRow.ParentFilter,
                    autoSelectFilterItemLabel: filterMetaRow.AutoSelectFilterItemLabel,
                    // NOTE: AutoSelectFilterItemId is set later, after filter items are created
                    subjectId: subject.Id),
                column: filterMetaRow.ColumnName))
            .ToList();
    }

    public List<(Indicator Indicator, string Column)> ReadIndicatorsFromCsv(
        IEnumerable<MetaRow> metaRows,
        Subject subject)
    {
        var indicatorRows = metaRows.Where(row => row.ColumnType == ColumnType.Indicator).ToList();

        indicatorRows.ForEach(row =>
        {
            if (string.IsNullOrWhiteSpace(row.IndicatorGrouping))
            {
                row.IndicatorGrouping = "Default";
            }
        });

        var indicatorGroups = indicatorRows
            .GroupBy(row => row.IndicatorGrouping!)
            .ToDictionary(
                rows => rows.Key,
                rows => new IndicatorGroup(rows.Key, subject.Id));

        return indicatorRows
            .Select(row =>
            {
                var indicatorGroup = indicatorGroups[row.IndicatorGrouping!];

                return (
                    indicator:
                    new Indicator
                    {
                        IndicatorGroup = indicatorGroup,
                        Label = row.Label,
                        Name = row.ColumnName,
                        Unit = row.IndicatorUnit,
                        DecimalPlaces = row.DecimalPlaces
                    },
                    column: row.ColumnName
                );
            })
            .ToList();
    }

    private string? ReadMetaColumnValue(MetaColumns column, IReadOnlyList<string> rowValues)
    {
        var columnIndex = _metaColumnIndexes[column];

        if (columnIndex == -1)
        {
            return null;
        }

        return rowValues[columnIndex].Trim().NullIfWhiteSpace();
    }
}
