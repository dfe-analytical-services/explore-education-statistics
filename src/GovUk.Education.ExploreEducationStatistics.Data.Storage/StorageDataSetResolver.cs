#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.EntityFrameworkCore;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage;

/// <summary>
/// Resolves a data set to the <see cref="IStorageDataSet" /> implementation for the
/// <see cref="DataStorageVersion" /> recorded against its data file.
///
/// Registered per scope, so each subject's data file is looked up at most once per request, and not at all when
/// the caller already holds the data file.
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

    public IStorageDataSet Resolve(File dataFile)
    {
        if (dataFile.Type != FileType.Data || dataFile.SubjectId is null)
        {
            throw new ArgumentException($"File {dataFile.Id} is not a data file linked to a subject", nameof(dataFile));
        }

        var subjectId = dataFile.SubjectId.Value;

        if (_resolved.TryGetValue(subjectId, out var dataSet) && dataSet is not null)
        {
            return dataSet;
        }

        dataSet =
            Create(subjectId, dataFile.DataStorageVersion)
            ?? throw new InvalidOperationException($"No data storage version recorded for data file {dataFile.Id}");

        _resolved[subjectId] = dataSet;

        return dataSet;
    }

    public async Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        if (_resolved.TryGetValue(subjectId, out var dataSet))
        {
            return dataSet;
        }

        var dataStorageVersion = await contentDbContext
            .Files.Where(file => file.SubjectId == subjectId && file.Type == FileType.Data)
            .Select(file => file.DataStorageVersion)
            .SingleOrDefaultAsync(cancellationToken);

        dataSet = Create(subjectId, dataStorageVersion);

        _resolved[subjectId] = dataSet;

        return dataSet;
    }

    private IStorageDataSet? Create(Guid subjectId, DataStorageVersion? dataStorageVersion)
    {
        return dataStorageVersion switch
        {
            DataStorageVersion.StatsDB => statisticsDbDataSetFactory.Create(subjectId),
            null => null,
            _ => throw new NotSupportedException($"Data storage version {dataStorageVersion} is not supported"),
        };
    }
}
