using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterGroupService : AbstractRepository<FilterGroup, long>, IFilterGroupService
    {
        public FilterGroupService(StatisticsDbContext context, ILogger<FilterGroupService> logger)
            : base(context, logger)
        {
        }
    }
}