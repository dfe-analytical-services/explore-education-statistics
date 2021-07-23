using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IGeoJsonRepository
    {
        GeoJson Find(long boundaryLevelId, string code);

        IEnumerable<GeoJson> Find(long boundaryLevelId, IEnumerable<string> codes);
    }
}