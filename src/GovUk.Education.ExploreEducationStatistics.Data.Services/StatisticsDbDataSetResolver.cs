#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class StatisticsDbDataSetResolver(IServiceProvider serviceProvider) : IStorageDataSetResolver
{
    public Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IStorageDataSet>(
            ActivatorUtilities.CreateInstance<StatisticsDbDataSet>(serviceProvider, subjectId)
        );
    }

    public async Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await Resolve(subjectId, cancellationToken);
    }
}
