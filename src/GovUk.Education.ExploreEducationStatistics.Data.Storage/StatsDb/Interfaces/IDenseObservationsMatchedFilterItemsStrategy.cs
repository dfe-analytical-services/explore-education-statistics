using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;

public interface IDenseObservationsMatchedFilterItemsStrategy
{
    Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    );
}
