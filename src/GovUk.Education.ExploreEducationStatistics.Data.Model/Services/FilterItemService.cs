using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterItemService : AbstractRepository<FilterItem, long>, IFilterItemService
    {
        public FilterItemService(ApplicationDbContext context,
            ILogger<FilterItemService> logger) : base(context, logger)
        {
        }

        public IEnumerable<FilterItem> GetFilterItems(SubjectMetaQueryContext query)
        {
            var filterItemIds = (from ofi in _context.Set<ObservationFilterItem>()
                join
                    o in _context.Observation.Where(query.ObservationPredicate()) 
                    on ofi.ObservationId equals o.Id
                select ofi.FilterItemId).Distinct().ToList();

            return DbSet()
                .AsNoTracking()
                .Where(item => filterItemIds.Contains(item.Id))
                .Include(item => item.FilterGroup.Filter);
        }
    }
}