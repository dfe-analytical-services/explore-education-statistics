using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ILocationService : IRepository<Location, long>
    {
        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            Expression<Func<Observation, bool>> observationPredicate);

        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IEnumerable<Observation> observations);
    }
}