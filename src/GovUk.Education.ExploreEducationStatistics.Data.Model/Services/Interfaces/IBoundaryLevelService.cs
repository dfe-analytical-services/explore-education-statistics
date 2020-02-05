using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IBoundaryLevelService : IRepository<BoundaryLevel, long>
    {
        IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels);
        BoundaryLevel FindLatestByGeographicLevel(GeographicLevel geographicLevel);
        IEnumerable<BoundaryLevel> FindRelatedByBoundaryLevel(long boundaryLevelId);
    }
}