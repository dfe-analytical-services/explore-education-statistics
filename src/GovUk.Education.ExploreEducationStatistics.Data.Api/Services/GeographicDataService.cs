using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class GeographicDataService : AbstractDataService<GeographicData>, IGeographicDataService
    {
        public GeographicDataService(ApplicationDbContext context, ILogger<GeographicDataService> logger) :
            base(context, logger)
        {
        }
    }
}