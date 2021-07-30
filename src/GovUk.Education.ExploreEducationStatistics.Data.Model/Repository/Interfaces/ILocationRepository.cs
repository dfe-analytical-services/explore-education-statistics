using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ILocationRepository : IRepository<Location, Guid>
    {
        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(Guid subjectId);

        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IQueryable<Observation> observations);

        IEnumerable<IObservationalUnit> GetObservationalUnits(GeographicLevel level, IEnumerable<string> codes);
    }
}