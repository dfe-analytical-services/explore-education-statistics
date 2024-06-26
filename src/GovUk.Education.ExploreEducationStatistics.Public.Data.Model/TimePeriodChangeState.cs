using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class TimePeriodChangeState : ITimePeriodDetails
{
    public required TimeIdentifier Code { get; set; }

    public required string Period { get; set; }
}
