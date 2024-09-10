using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessCompletionOfNextDataSetVersionFunction(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(nameof(ProcessCompletionOfNextDataSetVersion))]
    public async Task ProcessCompletionOfNextDataSetVersion(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessDataSetVersionContext input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessCompletionOfNextDataSetVersion));

        logger.LogInformation(
            "Processing completion of import for next data set version (InstanceId={InstanceId}, " +
            "DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId);

        try
        {
            await context.CallActivity(ActivityNames.UpdateFileStoragePath, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.ImportMetadata, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.ImportData, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.WriteDataFiles, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.CompleteNextDataSetVersionImportProcessing, logger,
                context.InstanceId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivity(ActivityNames.HandleProcessingFailure, logger, context.InstanceId);
        }
    }

    [Function(ActivityNames.UpdateFileStoragePath)]
    public async Task UpdateFileStoragePath(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        var currentLiveVersion = await publicDataDbContext
            .DataSets
            .Where(dataSet => dataSet.Id == dataSetVersion.DataSetId)
            .Select(dataSet => dataSet.LatestLiveVersion!)
            .SingleAsync(cancellationToken);

        var originalPath = dataSetVersionPathResolver.DirectoryPath(
            dataSetVersion: dataSetVersion,
            versionNumber: currentLiveVersion.DefaultNextVersion());

        var newPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        if (originalPath != newPath)
        {
            Directory.Move(sourceDirName: originalPath, destDirName: newPath);
        }
    }

    [Function(ActivityNames.CompleteNextDataSetVersionImportProcessing)]
    public async Task CompleteNextDataSetVersionImportProcessing(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.Completing, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        // Delete the DuckDb database file as it is no longer needed
        File.Delete(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));

        dataSetVersion.Status = DataSetVersionStatus.Draft;

        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
