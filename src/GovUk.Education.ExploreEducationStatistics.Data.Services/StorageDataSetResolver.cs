#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

/// <summary>
/// Resolves a data set to the <see cref="IStorageDataSet" /> implementation for the
/// <see cref="DataStorageVersion" /> recorded against its data file.
/// </summary>
public class StorageDataSetResolver(
    ContentDbContext contentDbContext,
    StatisticsDbDataSetResolver statisticsDbDataSetResolver,
    IDataFilesPathResolver dataFilesPathResolver,
    ILogger<ParquetV1DataSet> logger
) : IStorageDataSetResolver
{
    public async Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var dataFile =
            await contentDbContext
                .Files.AsNoTracking()
                .SingleOrDefaultAsync(
                    file => file.SubjectId == subjectId && file.Type == FileType.Data,
                    cancellationToken
                )
            ?? throw new InvalidOperationException($"No data file found for subject {subjectId}");

        return dataFile.DataStorageVersion switch
        {
            DataStorageVersion.StatsDB => await statisticsDbDataSetResolver.Resolve(subjectId, cancellationToken),
            DataStorageVersion.ParquetV1 => new ParquetV1DataSet(
                dataFile: dataFile,
                statisticsDbDataSet: await statisticsDbDataSetResolver.Resolve(subjectId, cancellationToken),
                dataFilesPathResolver: dataFilesPathResolver,
                logger: logger
            ),
            _ => throw new NotSupportedException(
                $"Data storage version {dataFile.DataStorageVersion} is not supported"
            ),
        };
    }
}
