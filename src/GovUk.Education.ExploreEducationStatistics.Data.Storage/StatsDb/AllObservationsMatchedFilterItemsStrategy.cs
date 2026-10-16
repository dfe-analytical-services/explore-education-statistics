using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

public class AllObservationsMatchedFilterItemsStrategy(StatisticsDbContext context)
    : IAllObservationsMatchedFilterItemsStrategy
{
    public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        CancellationToken cancellationToken
    )
    {
        return await context
            .FilterItem.AsNoTracking()
            .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
            .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
            .ToListAsync(cancellationToken);
    }
}
