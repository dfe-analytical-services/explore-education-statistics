using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class BoundaryLevelService : AbstractRepository<BoundaryLevel, long>, IBoundaryLevelService
    {
        public BoundaryLevelService(ApplicationDbContext context, ILogger<BoundaryLevelService> logger) 
            : base(context, logger)
        {
        }

        public BoundaryLevel FindLatestByGeographicLevel(GeographicLevel geographicLevel)
        {
            return DbSet()
                .Where(level => level.Level.Equals(geographicLevel))
                .OrderByDescending(level => level.Published)
                .First();
        }
    }
}