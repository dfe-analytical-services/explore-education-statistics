using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IGeoJsonService
    {
        GeoJson Find(long boundaryLevelId, string code);

        IQueryable<GeoJson> Find(long boundaryLevelId, IEnumerable<string> codes);
    }
}