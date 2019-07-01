using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IObservationService : IRepository<Observation, long>
    {
        IEnumerable<Observation> FindObservations(ObservationQueryContext query);

        IEnumerable<(TimeIdentifier TimeIdentifier, int Year)> GetTimePeriodsMeta(SubjectMetaQueryContext query);
    }
}