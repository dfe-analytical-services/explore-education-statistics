using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IDenseObservationsMatchedFilterItemsStrategy
{
    Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    );
}
