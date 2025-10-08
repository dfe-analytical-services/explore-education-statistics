using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface ISparseObservationsMatchedFilterItemsStrategy
{
    Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    );
}
