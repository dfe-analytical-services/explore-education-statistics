#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        public Dictionary<string, GeoJson> FindByBoundaryLevelAndCodes(long boundaryLevelId, IEnumerable<string> codes)
        {
            var codesList = codes.ToList();
            var cached = TryCacheLookup(boundaryLevelId, codesList);
            var cachedCodes = cached.Select(geoJson => geoJson.Code);
            var codesNotCached = codesList.Except(cachedCodes);

            var dbResult = FindAndCache(boundaryLevelId, codesNotCached);

            return cached
                .Union(dbResult)
                .ToDictionary(geoJson => geoJson.Code);
        }

        private IEnumerable<GeoJson> FindAndCache(long boundaryLevelId, IEnumerable<string> codes)
        {
            var geoJsonList = _context.GeoJson.Where(geoJson =>
                geoJson.BoundaryLevelId == boundaryLevelId &&
                codes.Contains(geoJson.Code)).ToList();

            AddAllToCache(boundaryLevelId, geoJsonList);

            return geoJsonList;
        }

        private List<GeoJson> TryCacheLookup(long boundaryLevelId, IEnumerable<string> codes)
        {
            return codes
                .Select(code => TryCacheLookup(boundaryLevelId, code))
                .WhereNotNull()
                .ToList();
        }

        private GeoJson? TryCacheLookup(long boundaryLevelId, string code)
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
