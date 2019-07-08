using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IObservationService : IRepository<Observation, long>
    {
        IEnumerable<Observation> FindObservations(ObservationQueryContext query);

        IEnumerable<(TimeIdentifier TimeIdentifier, int Year)> GetTimePeriodsMeta(SubjectMetaQueryContext query);
    }
}