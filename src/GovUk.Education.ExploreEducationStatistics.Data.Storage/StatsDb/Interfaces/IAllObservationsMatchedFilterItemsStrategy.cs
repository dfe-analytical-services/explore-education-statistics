using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;

public interface IAllObservationsMatchedFilterItemsStrategy
{
    Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        CancellationToken cancellationToken
    );
}
