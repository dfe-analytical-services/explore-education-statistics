using GovUk.Education.ExploreEducationStatistics.Model.Service;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions
{
    public static class TimePeriodExtensions
    {
        public static string GetTimePeriod(this (int Year, TimeIdentifier TimeIdentifier) tuple)
        {
            return $"{tuple.Year}_{tuple.TimeIdentifier.GetEnumValue()}";
        }

        public static string GetTimePeriod(this Observation observation)
        {
            return $"{observation.Year}_{observation.TimeIdentifier.GetEnumValue()}";
        }
    }
}