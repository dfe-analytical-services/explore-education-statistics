#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(Guid subjectId)
        {
            return _context.Observation
                .AsQueryable()
                .Where(observation => observation.SubjectId == subjectId)
                .Select(o => new {o.Year, o.TimeIdentifier})
                .Distinct()
                .AsNoTracking()
                .ToList()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .Select(tuple => (tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }

        public IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriods(
            IQueryable<Observation> observations)
        {
            return GetDistinctObservationTimePeriods(observations);
        }
        
        public IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(
            IList<Observation> observations)
        {
            var timePeriods = GetDistinctObservationTimePeriods(observations);

            var start = timePeriods.First();
            var end = timePeriods.Last();

            return TimePeriodUtil.GetTimePeriodRange(start, end);
        }

        public TimePeriodLabels GetTimePeriodLabels(Guid subjectId)
        {
            var observationsQuery = _context
                .Observation
                .AsQueryable()
                .Where(observation => observation.SubjectId == subjectId);
            
            var orderedTimePeriods = GetDistinctObservationTimePeriods(observationsQuery);

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

        private static IList<(int Year, TimeIdentifier TimeIdentifier)> GetDistinctObservationTimePeriods(
            IQueryable<Observation> observations)
        {
            return observations
                .Select(o => new {o.Year, o.TimeIdentifier})
                .Distinct()
                .AsNoTracking()
                .ToList()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .Select(tuple => (tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }
        
        private static IList<(int Year, TimeIdentifier TimeIdentifier)> GetDistinctObservationTimePeriods(
            IList<Observation> observations)
        {
            return observations
                .Select(o => new {o.Year, o.TimeIdentifier})
                .Distinct()
                .ToList()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .Select(tuple => (tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }
    }
}
