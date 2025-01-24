using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Models;

public record MetaFileRow
{
    public required string ColName { get; init; }

    public ColumnType ColType { get; init; }

    public required string Label { get; init; }

    public string? IndicatorGrouping { get; init; }

    public string? IndicatorUnit { get; init; }

    public IndicatorUnit? ParsedIndicatorUnit =>
        IndicatorUnit is not null
            ? EnumUtil.GetFromEnumValue<IndicatorUnit>(IndicatorUnit)
            : null;

    public byte? IndicatorDp { get; init; }

    public string? FilterGroupingColumn { get; init; }

    public string? FilterDefault { get; init; }

    public string? FilterHint { get; init; }

    public enum ColumnType
    {
        Indicator,
        Filter,
    }
}
