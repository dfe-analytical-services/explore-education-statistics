using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersionMetaSummary
{
    public required TimePeriodRange TimePeriodRange { get; set; }

    public required List<string> Filters { get; set; }

    public required List<string> Indicators { get; set; }

    public required List<GeographicLevel> GeographicLevels { get; set; }
}
