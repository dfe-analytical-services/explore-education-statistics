using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IObservationService
    {
        /// <summary>
        /// Finds <see cref="Observation">Observation rows</see> that match the given
        /// <see cref="ObservationQueryContext">Observation query</see> and stores the matching row Ids in the
        /// <see cref="MatchedObservation">#MatchedObservation temporary table.</see>
        /// This method then returns the query to select those matching Ids from the temporary table. This query can
        /// be used by client code to then quickly select against the matched Observations by making use of the
        /// populated temporary table results. 
        /// 
        /// </summary>
        /// <param name="query">The query to run in order to find matching Observation rows.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling this request.</param>
        /// <returns>A query for selecting the matched Observation Ids from the temporary table.</returns>
        Task<IQueryable<MatchedObservation>> GetMatchedObservations(
            ObservationQueryContext query,
            CancellationToken cancellationToken);
    }
}
