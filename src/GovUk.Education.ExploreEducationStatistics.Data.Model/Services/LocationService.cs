using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class LocationService : AbstractDataService<Location, long>, ILocationService
    {
        public LocationService(ApplicationDbContext context, ILogger<LocationService> logger) : base(context, logger)
        {
        }
    }
}