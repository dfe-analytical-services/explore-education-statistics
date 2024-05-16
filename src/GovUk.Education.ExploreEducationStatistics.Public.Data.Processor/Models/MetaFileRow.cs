using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;

public record MetaFileRow
{
    public required string ColName { get; init; }

    public ColumnType ColType { get; init; }

    public required string Label { get; init; }

    public string? IndicatorGrouping { get; init; }

    private string? _indicatorUnit { get; init; }

    public IndicatorUnit? IndicatorUnit =>
        _indicatorUnit is not null
            ? EnumUtil.GetFromEnumValue<IndicatorUnit>(_indicatorUnit)
            : null;

    public byte? IndicatorDp { get; init; }

    public string? FilterGroupingColumn { get; init; }

    public string? FilterHint { get; init; }

    public enum ColumnType
    {
        Indicator,
        Filter,
    }
}
