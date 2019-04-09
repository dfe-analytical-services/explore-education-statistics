using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class LevelService : AbstractDataService<LevelComposite, long>, ILevelService
    {
        public LevelService(ApplicationDbContext context, ILogger<LevelService> logger) : base(context, logger)
        {
        }
    }
}