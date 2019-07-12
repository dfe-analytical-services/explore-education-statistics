using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class GeoJsonService : IGeoJsonService
    {
        private readonly ApplicationDbContext _context;

        public GeoJsonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public GeoJson Find(GeographicLevel level, string code)
        {
            return _context.GeoJson.FirstOrDefault(geoJson =>
                geoJson.Code.Equals(code) && geoJson.GeographicLevel == level);
        }
    }
}