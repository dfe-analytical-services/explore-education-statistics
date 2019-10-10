using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class GeoJsonService : IGeoJsonService
    {
        private readonly StatisticsDbContext _context;

        public GeoJsonService(StatisticsDbContext context)
        {
            _context = context;
        }

        public GeoJson Find(long boundaryLevelId, string code)
        {
            return _context.GeoJson.FirstOrDefault(geoJson =>
                geoJson.BoundaryLevelId == boundaryLevelId &&
                geoJson.Code.Equals(code));
        }
        
        public IQueryable<GeoJson> Find(long boundaryLevelId, IEnumerable<string> codes)
        {
            return _context.GeoJson.Where(geoJson =>
                geoJson.BoundaryLevelId == boundaryLevelId &&
                codes.Contains(geoJson.Code));
        }
    }
}