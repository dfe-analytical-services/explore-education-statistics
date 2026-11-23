#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum MetaColumns
{
    col_name,
    col_type,
    label,
    filter_grouping_column,
    parent_filter,
    filter_hint,
    filter_default,
    indicator_grouping,
    indicator_unit,
    indicator_dp,
}

public class ImporterMetaService : IImporterMetaService
{
    private readonly IGuidGenerator _guidGenerator;

    public static readonly string[] RequiredMetaColumns = Enum.GetValues<MetaColumns>()
        .Where(col => col != MetaColumns.parent_filter && col != MetaColumns.filter_default) // if we make other columns optional, screener would also need updating
        .Select(col => col.ToString())
        .ToArray();

    public ImporterMetaService(IGuidGenerator guidGenerator)
    {
        _guidGenerator = guidGenerator;
    }

    public async Task<SubjectMeta> Import(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        IStorageDataSetWriter dataSetWriter
    )
    {
        var metaFileReader = new MetaDataFileReader(metaFileCsvHeaders);
        var metaRows = metaFileReader.GetMetaRows(metaFileRows);
        metaRows = IgnoreFiltersUsedForGrouping(metaRows);

        if (!await dataSetWriter.HasIndicators())
        {
            var indicators = metaFileReader
                .ReadIndicatorsFromCsv(metaRows, dataSetWriter.SubjectId)
                .Select(i => i.Indicator)
                .ToList();
            indicators.ForEach(indicator => indicator.Id = _guidGenerator.NewGuid());

            await dataSetWriter.WriteIndicators(indicators);
        }

        var filters = metaFileReader.ReadFiltersFromCsv(metaRows, dataSetWriter.SubjectId);
        filters.ForEach(filter => filter.Filter.Id = _guidGenerator.NewGuid());

        return new SubjectMeta { Filters = filters, Indicators = await GetIndicators(metaRows, dataSetWriter.DataSet) };
    }

    public async Task<SubjectMeta> GetSubjectMeta(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        IStorageDataSet dataSet
    )
    {
        var metaFileReader = new MetaDataFileReader(metaFileCsvHeaders);
        var metaRows = metaFileReader.GetMetaRows(metaFileRows);
        metaRows = IgnoreFiltersUsedForGrouping(metaRows);

        var filters = await GetFilters(metaRows, dataSet);
        var indicators = await GetIndicators(metaRows, dataSet);

        return new SubjectMeta { Filters = filters, Indicators = indicators };
    }

    private static async Task<List<(Filter Filter, string Column)>> GetFilters(
        IEnumerable<MetaRow> metaRows,
        IStorageDataSet dataSet
    )
    {
        var filters = await dataSet.ListFilters(includeItems: true);

        return metaRows
            .Where(row => row.ColumnType == ColumnType.Filter)
            .Select(filter => (filter: filters.Single(f => f.Name == filter.ColumnName), column: filter.ColumnName))
            .ToList();
    }

    private static List<MetaRow> IgnoreFiltersUsedForGrouping(List<MetaRow> metaRows)
    {
        var groupingColumns = metaRows.Select(mr => mr.FilterGroupingColumn?.ToLower()).WhereNotNull();

        return metaRows
            .Where(r => r.ColumnType != ColumnType.Filter || !groupingColumns.Contains(r.ColumnName.ToLower()))
            .ToList();
    }

    private static async Task<List<(Indicator Indicator, string Column)>> GetIndicators(
        IEnumerable<MetaRow> metaRows,
        IStorageDataSet dataSet
    )
    {
        var indicatorRows = metaRows.Where(row => row.ColumnType == ColumnType.Indicator).ToList();

        indicatorRows.ForEach(row =>
        {
            if (string.IsNullOrWhiteSpace(row.IndicatorGrouping))
            {
                row.IndicatorGrouping = "Default";
            }
        });

        var dataSetIndicatorGroups = await dataSet.ListIndicatorGroups();

        var indicatorGroups = indicatorRows
            .GroupBy(row => row.IndicatorGrouping!)
            .ToDictionary(
                rows => rows.Key,
                rows => dataSetIndicatorGroups.Single(ig => string.Equals(ig.Label, rows.Key, CurrentCultureIgnoreCase))
            );

        return indicatorRows
            .Select(row =>
            {
                var indicatorGroup = indicatorGroups[row.IndicatorGrouping!];

                return (
                    indicator: indicatorGroup.Indicators.Single(i =>
                        string.Equals(i.Label, row.Label, CurrentCultureIgnoreCase) && i.Unit == row.IndicatorUnit
                    ),
                    column: row.ColumnName
                );
            })
            .ToList();
    }
}
