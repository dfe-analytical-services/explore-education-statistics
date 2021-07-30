using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class GeoJsonRepository : IGeoJsonRepository
    {
        private readonly DataServiceMemoryCache<GeoJson> _cache;
        private readonly StatisticsDbContext _context;

        public GeoJsonRepository(StatisticsDbContext context,
            DataServiceMemoryCache<GeoJson> cache)
        {
            _context = context;
            _cache = cache;
        }

        public GeoJson Find(long boundaryLevelId, string code)
        {
            return _cache.GetOrCreate((boundaryLevelId, code), entry =>
                _context.GeoJson.FirstOrDefault(geoJson =>
                    geoJson.BoundaryLevelId == boundaryLevelId &&
                    geoJson.Code.Equals(code)));
        }

        public IEnumerable<GeoJson> Find(long boundaryLevelId, IEnumerable<string> codes)
        {
            var codesList = codes.ToList();
            var cached = TryCacheLookup(boundaryLevelId, codesList);
            var cachedCodes = cached.Select(geoJson => geoJson.Code);
            var codesNotCached = codesList.Except(cachedCodes);

            var dbResult = FindAndCache(boundaryLevelId, codesNotCached);
            return cached.Union(dbResult);
        }

        private IEnumerable<GeoJson> FindAndCache(long boundaryLevelId, IEnumerable<string> codes)
        {
            var geoJsonList = _context.GeoJson.Where(geoJson =>
                geoJson.BoundaryLevelId == boundaryLevelId &&
                codes.Contains(geoJson.Code)).ToList();

            AddAllToCache(boundaryLevelId, geoJsonList);

            return geoJsonList;
        }

        private IEnumerable<GeoJson> TryCacheLookup(long boundaryLevelId, IEnumerable<string> codes)
        {
            return codes.Select(code => TryCacheLookup(boundaryLevelId, code)).Where(geoJson => geoJson != null);
        }

        private GeoJson TryCacheLookup(long boundaryLevelId, string code)
        {
            return _cache.GetOrDefault($"{boundaryLevelId}_{code}");
        }

        private void AddAllToCache(long boundaryLevelId, List<GeoJson> geoJsonList)
        {
            foreach (var geoJson in geoJsonList)
            {
                _cache.Set($"{boundaryLevelId}_{geoJson.Code}", geoJson, TimeSpan.FromMinutes(30));
            }
        }
    }
}