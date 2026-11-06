#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

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

    public (List<Filter> Filters, List<Indicator> Indicators) ReadMetaCsv(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        Guid subjectId
    )
    {
        var metaFileReader = new MetaDataFileReader(metaFileCsvHeaders);
        var metaRows = metaFileReader.GetMetaRows(metaFileRows);
        metaRows = IgnoreFiltersUsedForGrouping(metaRows);

        // @MarkFix Make every id deterministic for newly imported parquet data sets, e.g. a name-based (v5) guid
        // from the subject id and the CSV column/label path, so a re-import gives the same ids. Covers filters and
        // indicators here, the Guid.NewGuid() in the IndicatorGroup, FilterGroup and FilterItem constructors, and
        // per-data-set location ids. Existing data sets keep their random ids.
        var indicators = metaFileReader.ReadIndicatorsFromCsv(metaRows, subjectId);
        indicators.ForEach(indicator => indicator.Id = _guidGenerator.NewGuid());

        var filters = metaFileReader.ReadFiltersFromCsv(metaRows, subjectId);
        filters.ForEach(filter => filter.Id = _guidGenerator.NewGuid());

        return (filters, indicators);
    }

    private static List<MetaRow> IgnoreFiltersUsedForGrouping(List<MetaRow> metaRows)
    {
        var groupingColumns = metaRows.Select(mr => mr.FilterGroupingColumn?.ToLower()).WhereNotNull();

        return metaRows
            .Where(r => r.ColumnType != ColumnType.Filter || !groupingColumns.Contains(r.ColumnName.ToLower()))
            .ToList();
    }
}
