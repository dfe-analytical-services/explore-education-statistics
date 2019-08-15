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
        public BoundaryLevelService(ApplicationDbContext context, ILogger<BoundaryLevelService> logger)
            : base(context, logger)
        {
        }

        public IEnumerable<BoundaryLevel> FindByGeographicLevel(GeographicLevel geographicLevel)
        {
            return FindMany(level => level.Level.Equals(geographicLevel))
                .OrderByDescending(level => level.Published);
        }

        public BoundaryLevel FindLatestByGeographicLevel(GeographicLevel geographicLevel)
        {
            return FindMany(level => level.Level.Equals(geographicLevel))
                .OrderByDescending(level => level.Published)
                .First();
        }

        public IEnumerable<BoundaryLevel> FindRelatedByBoundaryLevel(long boundaryLevelId)
        {
            var boundaryLevel = Find(boundaryLevelId);
            if (boundaryLevel == null)
            {
                throw new ArgumentException("Boundary Level does not exist", nameof(boundaryLevelId));
            }

            return FindByGeographicLevel(boundaryLevel.Level);
        }
    }
}