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
    public class FilterRepository : IFilterRepository
    {
        private readonly StatisticsDbContext _context;

        public FilterRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public Task<List<Filter>> GetFiltersIncludingItems(Guid subjectId)
        {
            return _context.Filter
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems)
                .Where(filter => filter.SubjectId == subjectId)
                .ToListAsync();
        }
    }
}
