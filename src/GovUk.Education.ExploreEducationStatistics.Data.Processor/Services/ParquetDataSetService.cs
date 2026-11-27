#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ParquetDataSetService(
    IDbContextSupplier dbContextSupplier,
    StatisticsDbDataSetFactory statisticsDbDataSetFactory,
    ParquetV1DataSetWriterFactory parquetV1DataSetWriterFactory,
    IParquetDataSetPathResolver pathResolver,
    ILogger<ParquetDataSetService> logger
) : IParquetDataSetService
{
    private const int ExportBatchSize = 10_000;
    private const string ExportLockFilename = "export.lock";
    private static readonly TimeSpan StaleExportLockAge = TimeSpan.FromHours(4);

    public async Task ExportStatsDbDataSet(Guid fileId, CancellationToken cancellationToken = default)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();

        var file = await contentDbContext.Files.SingleAsync(
            f => f.Id == fileId && f.Type == FileType.Data,
            cancellationToken
        );

        if (file.HasParquet)
        {
            logger.LogInformation("Data file {FileId} already has parquet files - skipping export", fileId);
            return;
        }

        if (file.DataStorageVersion != DataStorageVersion.StatsDB || file.SubjectId is null)
        {
            throw new InvalidOperationException(
                $"Data file {fileId} is not stored in StatsDB so can't be exported to parquet"
            );
        }

        var subjectId = file.SubjectId.Value;

        if (!TryAcquireExportLock(subjectId))
        {
            logger.LogInformation("Data file {FileId} is already being exported to parquet - skipping export", fileId);
            return;
        }

        try
        {
            await contentDbContext.Entry(file).ReloadAsync(cancellationToken);

            if (file.HasParquet)
            {
                logger.LogInformation("Data file {FileId} already has parquet files - skipping export", fileId);
                return;
            }

            await WriteParquetFiles(contentDbContext, file, subjectId, cancellationToken);
        }
        finally
        {
            System.IO.File.Delete(ExportLockPath(subjectId));
        }
    }

    private async Task WriteParquetFiles(
        ContentDbContext contentDbContext,
        File file,
        Guid subjectId,
        CancellationToken cancellationToken
    )
    {
        var fileId = file.Id;

        var statisticsDbDataSet = statisticsDbDataSetFactory.Create(subjectId);
        var dataSetWriter = parquetV1DataSetWriterFactory.Create(subjectId);

        var filters = await statisticsDbDataSet.ListFilters(includeItems: true, cancellationToken);
        var indicators = (await statisticsDbDataSet.ListIndicatorGroups(cancellationToken))
            .SelectMany(group => group.Indicators)
            .ToList();

        await dataSetWriter.WriteIndicators(indicators, cancellationToken);
        await dataSetWriter.WriteFilters(filters, cancellationToken);
        await dataSetWriter.WriteLocations(
            await statisticsDbDataSet.ListLocations(cancellationToken: cancellationToken),
            cancellationToken
        );

        await dataSetWriter.StartObservations(filters, indicators, resumeFromCsvRow: null, cancellationToken);

        // Older imports didn't record the filter of each observation's filter items.
        var filterIdsByFilterItemId = filters
            .SelectMany(filter =>
                filter
                    .FilterGroups.SelectMany(group => group.FilterItems)
                    .Select(item => (item.Id, FilterId: filter.Id))
            )
            .ToDictionary(tuple => tuple.Id, tuple => tuple.FilterId);

        await using var statisticsDbContext = dbContextSupplier.CreateDbContext<StatisticsDbContext>();

        // @MarkFix This isn't streamed. The context's retrying execution strategy makes EF buffer the whole result set,
        // which is a row per observation filter item for the entire data set. Page it by observation id instead, or
        // run it on a context without the retrying strategy.
        var observations = statisticsDbContext
            .Observation.AsNoTracking()
            .Include(observation => observation.FilterItems)
            .Where(observation => observation.SubjectId == subjectId)
            .AsAsyncEnumerable();

        var batch = new List<Observation>(ExportBatchSize);
        var exportedCount = 0L;

        await foreach (var observation in observations.WithCancellation(cancellationToken))
        {
            observation.FilterItems.ForEach(item => item.FilterId ??= filterIdsByFilterItemId[item.FilterItemId]);
            batch.Add(observation);

            if (batch.Count == ExportBatchSize)
            {
                await dataSetWriter.AppendObservations(batch);
                exportedCount += batch.Count;
                batch.Clear();

                logger.LogInformation("Staged {Count} observations for data file {FileId}", exportedCount, fileId);
            }
        }

        if (batch.Count > 0)
        {
            await dataSetWriter.AppendObservations(batch);
        }

        var statsDbCount = await statisticsDbContext.Observation.LongCountAsync(
            observation => observation.SubjectId == subjectId,
            cancellationToken
        );
        var stagedCount = await dataSetWriter.CountObservations(cancellationToken);

        if (stagedCount != statsDbCount)
        {
            throw new InvalidOperationException(
                $"Staged {stagedCount} observations for data file {fileId} but StatsDB has {statsDbCount}"
            );
        }

        await dataSetWriter.Complete(cancellationToken);

        // @MarkFix The file stays DataStorageVersion.StatsDB, so deleting or replacing it only runs
        // StatisticsDbDataSet.Delete, which removes the SQL rows and leaves data-files/{subjectId} on the file share.
        // Delete the parquet directory too when a StatsDB data set with HasParquet is deleted.
        file.HasParquet = true;
        await contentDbContext.SaveChangesAsync(cancellationToken);

        await dataSetWriter.CleanUp(cancellationToken);
    }

    private bool TryAcquireExportLock(Guid subjectId)
    {
        var lockPath = ExportLockPath(subjectId);

        Directory.CreateDirectory(pathResolver.DirectoryPath(subjectId));

        if (
            System.IO.File.Exists(lockPath)
            && DateTime.UtcNow - System.IO.File.GetLastWriteTimeUtc(lockPath) > StaleExportLockAge
        )
        {
            logger.LogWarning("Removing stale parquet export lock for subject {SubjectId}", subjectId);
            System.IO.File.Delete(lockPath);
        }

        try
        {
            using var _ = new FileStream(lockPath, FileMode.CreateNew);
            return true;
        }
        catch (IOException) when (System.IO.File.Exists(lockPath))
        {
            return false;
        }
    }

    private string ExportLockPath(Guid subjectId) =>
        Path.Combine(pathResolver.DirectoryPath(subjectId), ExportLockFilename);
}
