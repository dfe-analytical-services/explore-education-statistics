#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TimePeriodService : ITimePeriodService
    {
        private readonly StatisticsDbContext _context;

        public TimePeriodService(StatisticsDbContext context)
        {
            _context = context;
        }

        public Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetTimePeriods(Guid subjectId)
        {
            var observationsQuery = _context
                .Observation
                .AsNoTracking()
                .Where(observation => observation.SubjectId == subjectId);

            return GetDistinctObservationTimePeriods(observationsQuery);
        }

        public Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetTimePeriods(
            IQueryable<Observation> observationsQuery)
        {
            return GetDistinctObservationTimePeriods(observationsQuery);
        }

        public IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(
            IList<Observation> observations)
        {
            var timePeriods = GetDistinctObservationTimePeriods(observations);

            var start = timePeriods.First();
            var end = timePeriods.Last();

            return TimePeriodUtil.GetTimePeriodRange(start, end);
        }

        public async Task<TimePeriodLabels> GetTimePeriodLabels(Guid subjectId)
        {
            var orderedTimePeriods = await GetTimePeriods(subjectId);

            if (!orderedTimePeriods.Any())
            {
                return new TimePeriodLabels();
            }

            var first = orderedTimePeriods.First();
            var last = orderedTimePeriods.Last();

            return new TimePeriodLabels(
                TimePeriodLabelFormatter.Format(first.Year, first.TimeIdentifier),
                TimePeriodLabelFormatter.Format(last.Year, last.TimeIdentifier));
        }

        private static async Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetDistinctObservationTimePeriods(
            IQueryable<Observation> observationsQuery)
        {
            var timePeriods = (await observationsQuery
                    .Select(o => new { o.Year, o.TimeIdentifier })
                    .Distinct()
                    .ToListAsync())
                .Select(tuple => (tuple.Year, tuple.TimeIdentifier));

            return OrderTimePeriods(timePeriods);
        }

        private static IList<(int Year, TimeIdentifier TimeIdentifier)> GetDistinctObservationTimePeriods(
            IList<Observation> observations)
        {
            var timePeriods = observations
                .Select(o => (o.Year, o.TimeIdentifier))
                .Distinct();

            return OrderTimePeriods(timePeriods);
        }

        private static List<(int Year, TimeIdentifier TimeIdentifier)> OrderTimePeriods(
            IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> timePeriods)
        {
            // Ordering of time periods must be evaluated in memory rather than being translated to a database query.
            // They are expected to be ordered by their definition order, not by their enum value
            return timePeriods
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .ToList();
        }
    }
}
