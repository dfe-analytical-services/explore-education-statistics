using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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

        public IEnumerable<FilterItem> GetFilterItems(IQueryable<Observation> observations)
        {
            return observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem).Distinct();
        }

        public IEnumerable<FilterItem> GetFilterItemsIncludingFilters(IQueryable<Observation> observations)
        {
            return observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem)
                .Distinct()
                .Include(item => item.FilterGroup.Filter);
        }
    }
}