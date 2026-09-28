#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

/// <summary>
/// Resolves a data set to the <see cref="IStorageDataSet" /> implementation for the
/// <see cref="DataStorageVersion" /> recorded against its data file.
/// </summary>
public class StorageDataSetResolver(
    ContentDbContext contentDbContext,
    StatisticsDbDataSetResolver statisticsDbDataSetResolver
) : IStorageDataSetResolver
{
    public async Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var dataStorageVersion = await contentDbContext
            .Files.Where(file => file.SubjectId == subjectId && file.Type == FileType.Data)
            .Select(file => (DataStorageVersion?)file.DataStorageVersion)
            .SingleOrDefaultAsync(cancellationToken);

        return dataStorageVersion switch
        {
            DataStorageVersion.StatsDB => await statisticsDbDataSetResolver.Resolve(subjectId, cancellationToken),
            null => throw new InvalidOperationException($"No data file found for subject {subjectId}"),
            _ => throw new NotSupportedException($"Data storage version {dataStorageVersion} is not supported"),
        };
    }
}
