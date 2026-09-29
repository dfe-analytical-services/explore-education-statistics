#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class StatisticsDbDataSetResolver(
    StatisticsDbContext context,
    IObservationService observationService,
    IAllObservationsMatchedFilterItemsStrategy allObservationsMatchedFilterItemsStrategy,
    ISparseObservationsMatchedFilterItemsStrategy sparseObservationsMatchedFilterItemsStrategy,
    IDenseObservationsMatchedFilterItemsStrategy denseObservationsMatchedFilterItemsStrategy,
    ILogger<StatisticsDbDataSet> logger
) : IStorageDataSetResolver
{
    public Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IStorageDataSet>(
            new StatisticsDbDataSet(
                subjectId: subjectId,
                context: context,
                observationService: observationService,
                allObservationsMatchedFilterItemsStrategy: allObservationsMatchedFilterItemsStrategy,
                sparseObservationsMatchedFilterItemsStrategy: sparseObservationsMatchedFilterItemsStrategy,
                denseObservationsMatchedFilterItemsStrategy: denseObservationsMatchedFilterItemsStrategy,
                logger: logger
            )
        );
    }

    public async Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await Resolve(subjectId, cancellationToken);
    }
}
