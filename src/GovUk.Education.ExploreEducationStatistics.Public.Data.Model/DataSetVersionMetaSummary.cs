using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersionMetaSummary
{
    public TimePeriodRange TimePeriodRange { get; set; } = null!;

    public List<string> Filters { get; set; } = new();

    public List<string> Indicators { get; set; } = new();

    public List<GeographicLevel> GeographicLevels { get; set; } = new();
}
