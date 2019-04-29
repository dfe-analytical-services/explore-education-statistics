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

        public IEnumerable<Filter> GetFiltersBySubjectId(long subjectId)
        {
            return DbSet().Where(filter => filter.SubjectId == subjectId)
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems);
        }
    }
}