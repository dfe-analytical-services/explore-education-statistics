using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions
{
    public static class TimePeriodExtensions
    {
        public static (int Year, TimeIdentifier TimeIdentifier) GetTimePeriod(this Observation observation)
        {
            return (observation.Year, observation.TimeIdentifier);
        }
    }
}