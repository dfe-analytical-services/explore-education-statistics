using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IObservationService : IRepository<Observation, long>
    {
        Task<IList<Observation>> FindObservations(
            ObservationQueryContext query,
            CancellationToken cancellationToken = default);

        IQueryable<Observation> FindObservations(SubjectMetaQueryContext query);
    }
}
