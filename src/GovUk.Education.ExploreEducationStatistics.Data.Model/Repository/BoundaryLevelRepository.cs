#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class BoundaryLevelRepository : AbstractRepository<BoundaryLevel, long>, IBoundaryLevelRepository
    {
        private readonly DataServiceMemoryCache<BoundaryLevel> _cache;

        public BoundaryLevelRepository(
            StatisticsDbContext context,
            DataServiceMemoryCache<BoundaryLevel> cache)
            : base(context)
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

        public BoundaryLevel? FindLatestByGeographicLevel(GeographicLevel geographicLevel)
        {
            var boundaryLevel = FindMany(level => level.Level == geographicLevel)
                .OrderByDescending(level => level.Published)
                .FirstOrDefault();

            return boundaryLevel;
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
