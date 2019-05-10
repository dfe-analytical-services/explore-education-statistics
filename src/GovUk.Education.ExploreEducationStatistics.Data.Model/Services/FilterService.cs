using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterService : AbstractDataService<Filter, long>, IFilterService
    {
        public FilterService(ApplicationDbContext context,
            ILogger<FilterService> logger) : base(context, logger)
        {
        }

        public IEnumerable<Filter> GetFilters(long subjectId, IEnumerable<int> years = null)
        {
            // TODO DFE-609 years is ignored
            
            return DbSet().AsNoTracking().Where(filter => filter.SubjectId == subjectId)
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems);
        }
    }
}