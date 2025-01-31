using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;

public record DataSetVersionMappingMeta
{
    public required IDictionary<FilterMeta, List<FilterOptionMeta>> Filters { get; init; }

    public required IDictionary<LocationMeta, List<LocationOptionMetaRow>> Locations { get; init; }

    public required DataSetVersionMetaSummary MetaSummary { get; init; }

    public required IList<IndicatorMeta> Indicators { get; init; }

    public required GeographicLevelMeta GeographicLevel { get; init; }

    public required IList<TimePeriodMeta> TimePeriods { get; init; }
}
