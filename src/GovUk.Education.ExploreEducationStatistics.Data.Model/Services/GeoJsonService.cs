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

        public GeoJson Find(long boundaryLevelId, string code)
        {
            return _context.GeoJson.FirstOrDefault(geoJson =>
                geoJson.BoundaryLevelId == boundaryLevelId &&
                geoJson.Code.Equals(code));
        }
    }
}