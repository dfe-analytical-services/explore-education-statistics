using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class TimePeriodChangeState
{
    public required TimeIdentifier Code { get; set; }

    public required int Year { get; set; }
}
