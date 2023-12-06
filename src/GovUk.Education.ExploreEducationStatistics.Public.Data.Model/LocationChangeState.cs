using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationChangeState
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    public string Code { get; set; } = string.Empty;

    public GeographicLevel Level { get; set; }
}
