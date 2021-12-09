#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IGeoJsonRepository
    {
        Dictionary<string, GeoJson> FindByBoundaryLevelAndCodes(long boundaryLevelId, IEnumerable<string> codes);
    }
}
