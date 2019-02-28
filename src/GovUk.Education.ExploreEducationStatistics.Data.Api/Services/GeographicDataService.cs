using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class GeographicDataService : AbstractDataService<GeographicData>, IGeographicDataService
    {   
        public GeographicDataService(ApplicationDbContext context) : base(context)
        {
        }
    }
}