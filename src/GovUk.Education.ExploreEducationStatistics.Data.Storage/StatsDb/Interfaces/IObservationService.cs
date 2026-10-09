#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;

public interface IObservationService
{
    /// <summary>
    /// Finds the subject's <see cref="Observation" /> rows at the given locations within the time period that have
    /// every one of the given filter items, and stores the matching row Ids in the
    /// <see cref="MatchedObservation" /> temporary table.
    /// This method then returns a reference to the temporary table. Client code can then quickly select against
    /// the matched Observations by making use of the populated temporary table results.
    /// </summary>
    /// <returns>A reference to the temporary table holding the matched Observation Ids.</returns>
    Task<ITempTableReference> GetMatchedObservations(
        Guid subjectId,
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        CancellationToken cancellationToken = default
    );
}
