using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class BoundaryLevelService : AbstractRepository<BoundaryLevel, long>, IBoundaryLevelService
    {
        private readonly DataServiceMemoryCache<BoundaryLevel> _cache;

        public BoundaryLevelService(StatisticsDbContext context,
            DataServiceMemoryCache<BoundaryLevel> cache,
            ILogger<BoundaryLevelService> logger)
            : base(context, logger)
        {
            _cache = cache;
        }

        private IEnumerable<BoundaryLevel> FindByGeographicLevel(GeographicLevel geographicLevel)
        {
            return FindMany(level => level.Level == geographicLevel)
                .OrderByDescending(level => level.Published);
        }

        public IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels)
        {
            return FindMany(level => geographicLevels.Contains(level.Level))
                .OrderByDescending(level => level.Published);
        }

        public BoundaryLevel FindLatestByGeographicLevel(GeographicLevel geographicLevel)
        {
            return FindMany(level => level.Level == geographicLevel)
                .OrderByDescending(level => level.Published)
                .First();
        }

        public IEnumerable<BoundaryLevel> FindRelatedByBoundaryLevel(long boundaryLevelId)
        {
            var boundaryLevel = _cache.GetOrCreate(boundaryLevelId, cacheEntry => Find(boundaryLevelId));
            if (boundaryLevel == null)
            {
                throw new ArgumentException("Boundary Level does not exist", nameof(boundaryLevelId));
            }

            return FindByGeographicLevel(boundaryLevel.Level);
        }
    }
}