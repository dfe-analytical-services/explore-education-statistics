using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class IndicatorRepository : AbstractRepository<Indicator, Guid>, IIndicatorRepository
    {
        public IndicatorRepository(StatisticsDbContext context) : base(context)
        {
        }

        public IEnumerable<Indicator> GetIndicators(Guid subjectId)
        {
            return DbSet()
                .AsNoTracking()
                .Join(_context.IndicatorGroup, indicator => indicator.IndicatorGroupId,
                    indicatorGroup => indicatorGroup.Id,
                    (indicator, indicatorGroup) => new {indicator, indicatorGroup})
                .Where(t => t.indicatorGroup.SubjectId == subjectId)
                .Select(t => t.indicator);
        }

        public IEnumerable<Indicator> GetIndicators(Guid subjectId, IEnumerable<Guid> indicatorIds = null)
        {
            if (indicatorIds == null || !indicatorIds.Any())
            {
                return GetIndicators(subjectId);
            }

            return DbSet()
                .AsNoTracking()
                .Join(_context.IndicatorGroup, indicator => indicator.IndicatorGroupId,
                    indicatorGroup => indicatorGroup.Id,
                    (indicator, indicatorGroup) => new {indicator, indicatorGroup})
                .Where(t => indicatorIds.Contains(t.indicator.Id))
                .Where(t => t.indicatorGroup.SubjectId == subjectId)
                .Select(t => t.indicator);
        }
    }
}