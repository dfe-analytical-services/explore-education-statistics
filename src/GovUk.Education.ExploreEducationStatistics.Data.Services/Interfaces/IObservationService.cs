using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IObservationService : IRepository<Observation, long>
    {
        IEnumerable<Observation> FindObservations(ObservationQueryContext query);

        IEnumerable<Observation> FindObservations(SubjectMetaQueryContext query);
    }
}