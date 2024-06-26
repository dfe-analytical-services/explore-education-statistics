using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

public interface ITimePeriodDetails
{
    TimeIdentifier Code { get; set; }

    string Period { get; set; }
}
