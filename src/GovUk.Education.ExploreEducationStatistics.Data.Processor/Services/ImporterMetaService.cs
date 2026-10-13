#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

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
        IStorageDataSet dataSet,
        IStorageDataSetWriter dataSetWriter
    )
    {
        var metaFileReader = new MetaDataFileReader(metaFileCsvHeaders);
        var metaRows = metaFileReader.GetMetaRows(metaFileRows);
        metaRows = IgnoreFiltersUsedForGrouping(metaRows);

        var filtersAndMeta = metaFileReader.ReadFiltersFromCsv(metaRows, dataSet.SubjectId);
        var indicatorsAndMeta = metaFileReader.ReadIndicatorsFromCsv(metaRows, dataSet.SubjectId);

        var filtersAlreadyImported = filtersAndMeta.Count > 0 && (await dataSet.ListFilters()).Count > 0;

        var indicatorsAlreadyImported = indicatorsAndMeta.Count > 0 && (await dataSet.ListIndicatorGroups()).Count > 0;

        if (!filtersAlreadyImported || !indicatorsAlreadyImported)
        {
            var filters = filtersAndMeta.Select(f => f.Filter).ToList();
            filters.ForEach(filter => filter.Id = _guidGenerator.NewGuid());

            var indicators = indicatorsAndMeta.Select(i => i.Indicator).ToList();
            indicators.ForEach(indicator => indicator.Id = _guidGenerator.NewGuid());

            await dataSetWriter.WriteFiltersAndIndicators(filters, indicators);
        }

        return await GetSubjectMeta(metaFileCsvHeaders, metaFileRows, dataSet);
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
            .ToDictionary(rows => rows.Key, rows => dataSetIndicatorGroups.Single(ig => ig.Label == rows.Key));

        return indicatorRows
            .Select(row =>
            {
                var indicatorGroup = indicatorGroups[row.IndicatorGrouping!];

                return (
                    indicator: indicatorGroup.Indicators.Single(i =>
                        i.Label == row.Label && i.Unit == row.IndicatorUnit
                    ),
                    column: row.ColumnName
                );
            })
            .ToList();
    }
}
