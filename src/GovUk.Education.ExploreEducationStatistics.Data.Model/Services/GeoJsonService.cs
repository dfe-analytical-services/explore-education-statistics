using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class GeoJsonService : IGeoJsonService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public GeoJsonService(ApplicationDbContext context,
            ILogger<GeoJsonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public GeoJson Find(string code)
        {
            return _context.GeoJson.FirstOrDefault(geoJson => geoJson.Code == code);
        }
    }
}