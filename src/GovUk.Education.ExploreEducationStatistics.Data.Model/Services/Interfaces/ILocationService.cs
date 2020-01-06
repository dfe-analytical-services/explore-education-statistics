using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ILocationService : IRepository<Location, Guid>
    {
        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(Guid subjectId);
        
        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IQueryable<Observation> observations);
    }
}