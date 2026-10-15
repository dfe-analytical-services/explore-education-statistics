#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ParquetDataSetService(
    IDbContextSupplier dbContextSupplier,
    ParquetV1DataSetWriterFactory parquetV1DataSetWriterFactory,
    ILogger<ParquetDataSetService> logger
) : IParquetDataSetService
{
    private const int ExportBatchSize = 10_000;

    public async Task SetHasParquet(Guid fileId, CancellationToken cancellationToken = default)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();

        var file = await contentDbContext.Files.SingleAsync(f => f.Id == fileId, cancellationToken);
        file.HasParquet = true;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

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

        var dataSetWriter = parquetV1DataSetWriterFactory.Create(subjectId);

        var filters = await dataSetWriter.DataSet.ListFilters(includeItems: true, cancellationToken);
        var indicators = (await dataSetWriter.DataSet.ListIndicatorGroups(cancellationToken))
            .SelectMany(group => group.Indicators)
            .ToList();

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
                await dataSetWriter.WriteObservations(batch, cancellationToken);
                exportedCount += batch.Count;
                batch.Clear();

                logger.LogInformation("Staged {Count} observations for data file {FileId}", exportedCount, fileId);
            }
        }

        if (batch.Count > 0)
        {
            await dataSetWriter.WriteObservations(batch, cancellationToken);
        }

        await dataSetWriter.Complete(cancellationToken);

        file.HasParquet = true;
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }
}
