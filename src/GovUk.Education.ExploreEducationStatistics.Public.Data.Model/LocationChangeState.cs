using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationChangeState
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    public required string Code { get; set; }

    public required GeographicLevel Level { get; set; }
}
