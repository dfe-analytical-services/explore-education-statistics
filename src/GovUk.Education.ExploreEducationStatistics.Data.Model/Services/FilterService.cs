using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterService : AbstractDataService<Filter, long>, IFilterService
    {
        public FilterService(ApplicationDbContext context,
            ILogger<FilterService> logger) : base(context, logger)
        {
        }
    }
}