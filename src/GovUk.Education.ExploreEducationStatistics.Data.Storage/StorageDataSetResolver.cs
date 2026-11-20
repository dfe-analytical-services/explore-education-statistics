#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.EntityFrameworkCore;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage;

public class StorageDataSetResolver(
    ContentDbContext contentDbContext,
    StatisticsDbDataSetFactory statisticsDbDataSetFactory
) : IStorageDataSetResolver
{
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

        return CreateStorageDataSet(subjectId, dataFile.DataStorageVersion)
            ?? throw new InvalidOperationException($"No data storage version recorded for data file {dataFile.Id}");
    }

    public async Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var dataStorageVersion = await contentDbContext
            .Files.Where(file => file.SubjectId == subjectId && file.Type == FileType.Data)
            .Select(file => file.DataStorageVersion)
            .SingleOrDefaultAsync(cancellationToken);

        return CreateStorageDataSet(subjectId, dataStorageVersion);
    }

    private IStorageDataSet? CreateStorageDataSet(Guid subjectId, DataStorageVersion? dataStorageVersion)
    {
        return dataStorageVersion switch
        {
            DataStorageVersion.StatsDB => statisticsDbDataSetFactory.Create(subjectId),
            null => null,
            _ => throw new NotSupportedException($"Data storage version {dataStorageVersion} is not supported"),
        };
    }
}
