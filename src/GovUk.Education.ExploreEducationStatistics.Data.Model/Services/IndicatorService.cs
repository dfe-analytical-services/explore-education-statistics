using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class IndicatorService : AbstractDataService<Indicator, long>, IIndicatorService
    {
        public IndicatorService(ApplicationDbContext context, ILogger<IndicatorService> logger)
            : base(context, logger)
        {
        }
    }
}