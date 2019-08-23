using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TimePeriodService : ITimePeriodService
    {
        public IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(
            IQueryable<Observation> observations)
        {
            return GetDistinctObservationTimePeriods(observations);
        }

        public IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(
            IQueryable<Observation> observations)
        {
            var timePeriods = GetDistinctObservationTimePeriods(observations).ToList();

            var start = timePeriods.First();
            var end = timePeriods.Last();

            if (start.TimeIdentifier.IsNumberOfTerms() || end.TimeIdentifier.IsNumberOfTerms())
            {
                return MergeTimePeriodsWithHalfTermRange(timePeriods, start.Year, end.Year);
            }

            return TimePeriodUtil.GetTimePeriodRange(start, end);
        }

        private static IEnumerable<(int Year, TimeIdentifier TimeIdentifier)>
            MergeTimePeriodsWithHalfTermRange(
                List<(int Year, TimeIdentifier TimeIdentifier)> timePeriods, int startYear, int endYear)
        {
            // Generate a year range based only on Six Half Terms
            var range = TimePeriodUtil.Range(startYear, SixHalfTerms, endYear, SixHalfTerms);

            // Merge it with the distinct time periods to replace any years which should be Five Half Terms
            var rangeMap = range.ToDictionary(tuple => tuple.Year, tuple => tuple);
            timePeriods.ForEach(tuple => { rangeMap[tuple.Year] = (tuple.Year, tuple.TimeIdentifier); });

            return rangeMap.Values;
        }

        private static IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetDistinctObservationTimePeriods(
            IQueryable<Observation> observations)
        {
            return observations.Select(o => new {o.Year, o.TimeIdentifier})
                .Distinct()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .ToList()
                .Select(tuple => (tuple.Year, tuple.TimeIdentifier));
        }
    }
}