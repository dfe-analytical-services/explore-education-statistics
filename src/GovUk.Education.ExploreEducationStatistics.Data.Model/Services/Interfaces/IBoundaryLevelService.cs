using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IBoundaryLevelService : IRepository<BoundaryLevel, long>
    {
        IEnumerable<BoundaryLevel> FindByGeographicLevel(GeographicLevel geographicLevel);
        BoundaryLevel FindLatestByGeographicLevel(GeographicLevel geographicLevel);
        IEnumerable<BoundaryLevel> FindRelatedByBoundaryLevel(long boundaryLevelId);
    }
}