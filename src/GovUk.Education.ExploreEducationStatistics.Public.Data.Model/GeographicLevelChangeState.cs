using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class GeographicLevelChangeState
{
    public required string Id { get; set; }

    public required GeographicLevel Level { get; set; }
}
