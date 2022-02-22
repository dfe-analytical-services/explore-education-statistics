#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IBoundaryLevelRepository : IRepository<BoundaryLevel, long>
    {
        IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels);
        BoundaryLevel? FindLatestByGeographicLevel(GeographicLevel geographicLevel);
        // TODO DW - remove
        IEnumerable<BoundaryLevel> FindRelatedByBoundaryLevel(long boundaryLevelId);
    }
}
