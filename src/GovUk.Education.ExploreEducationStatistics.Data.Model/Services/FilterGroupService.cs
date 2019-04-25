using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterGroupService : AbstractDataService<FilterGroup, long>, IFilterGroupService
    {
        public FilterGroupService(ApplicationDbContext context, ILogger<FilterGroupService> logger)
            : base(context, logger)
        {
        }
    }
}