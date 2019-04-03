using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class GeographicDataService : AbstractDataService<GeographicData, long>, IGeographicDataService
    {
        public GeographicDataService(ApplicationDbContext context, ILogger<GeographicDataService> logger) :
            base(context, logger)
        {
        }
    }
}