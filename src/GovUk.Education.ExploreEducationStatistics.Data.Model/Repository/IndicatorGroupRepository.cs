#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class IndicatorGroupRepository : IIndicatorGroupRepository
    {
        private readonly StatisticsDbContext _context;

        public IndicatorGroupRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<List<IndicatorGroup>> GetIndicatorGroups(Guid subjectId)
        {
            return await _context.IndicatorGroup
                .Include(group => group.Indicators)
                .Where(indicatorGroup => indicatorGroup.SubjectId == subjectId)
                .ToListAsync();
        }
    }
}
