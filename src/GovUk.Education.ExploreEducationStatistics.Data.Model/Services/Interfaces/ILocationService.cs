using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ILocationService : IRepository<Location, long>
    {
        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            SubjectMetaQueryContext query);

        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IEnumerable<Location> locations);
    }
}