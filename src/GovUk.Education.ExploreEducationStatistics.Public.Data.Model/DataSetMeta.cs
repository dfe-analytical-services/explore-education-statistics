using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required List<FilterMeta> Filters { get; set; } = new();

    public required List<IndicatorMeta> Indicators { get; set; } = new();

    public required List<TimePeriodMeta> TimePeriods { get; set; } = new();

    public required List<LocationMeta> Locations { get; set; } = new();

    public required List<GeographicLevel> GeographicLevels { get; set; } = new();

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}
