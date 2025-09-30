using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public record DataSetVersionMeta
{
    public IEnumerable<FilterMeta>? FilterMetas { get; init; }

    public IEnumerable<LocationMeta>? LocationMetas { get; init; }

    public GeographicLevelMeta? GeographicLevelMeta { get; init; }

    public IEnumerable<IndicatorMeta>? IndicatorMetas { get; init; }

    public IEnumerable<TimePeriodMeta>? TimePeriodMetas { get; init; }
}
