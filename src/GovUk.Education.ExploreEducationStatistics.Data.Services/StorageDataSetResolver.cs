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
///
/// Registered per scope, so each subject's data file is looked up at most once per request.
/// </summary>
public class StorageDataSetResolver(
    ContentDbContext contentDbContext,
    StatisticsDbDataSetFactory statisticsDbDataSetFactory
) : IStorageDataSetResolver
{
    private readonly Dictionary<Guid, IStorageDataSet?> _resolved = [];

    public async Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await TryResolve(subjectId, cancellationToken)
            ?? throw new InvalidOperationException($"No data file found for subject {subjectId}");
    }

    public async Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        if (_resolved.TryGetValue(subjectId, out var dataSet))
        {
            return dataSet;
        }

        var dataStorageVersion = await contentDbContext
            .Files.Where(file => file.SubjectId == subjectId && file.Type == FileType.Data)
            .Select(file => (DataStorageVersion?)file.DataStorageVersion)
            .SingleOrDefaultAsync(cancellationToken);

        dataSet = dataStorageVersion switch
        {
            DataStorageVersion.StatsDB => statisticsDbDataSetFactory.Create(subjectId),
            null => null,
            _ => throw new NotSupportedException($"Data storage version {dataStorageVersion} is not supported"),
        };

        _resolved[subjectId] = dataSet;

        return dataSet;
    }
}
