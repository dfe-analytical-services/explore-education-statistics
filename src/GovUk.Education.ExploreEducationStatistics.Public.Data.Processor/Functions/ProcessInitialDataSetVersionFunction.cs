using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessInitialDataSetVersionFunction(PublicDataDbContext publicDataDbContext)
{
    [Function(nameof(ProcessInitialDataSetVersion))]
    public async Task ProcessInitialDataSetVersion(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessInitialDataSetVersionContext input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessInitialDataSetVersion));

        logger.LogInformation(
            "Processing initial data set version (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId);

        try
        {
            // Other activity function calls to be added here to cover the following stages:
            // Move CSV files from Azure Blob Storage to Azure File Share
            // Create meta summary for the DataSetVersion
            // Import metadata to DuckDb
            // Import data to DuckDb
            // Export to Parquet files

            await context.CallActivityAsync(nameof(CompleteProcessing), input);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(CompleteProcessing),
                context.InstanceId,
                input.DataSetVersionId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivityAsync(nameof(HandleProcessingFailure), input);
        }
    }

    [Function(nameof(CompleteProcessing))]
    public async Task CompleteProcessing(
        [ActivityTrigger] ProcessInitialDataSetVersionContext input,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: input.DataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);

        dataSetVersionImport.Stage = DataSetVersionImportStage.Completing;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        // Any additional logic to tidy up after importing will be added here

        dataSetVersion.Status = DataSetVersionStatus.Draft;
        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    [Function(nameof(HandleProcessingFailure))]
    public async Task HandleProcessingFailure(
        [ActivityTrigger] ProcessInitialDataSetVersionContext input,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: input.DataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        dataSetVersion.Status = DataSetVersionStatus.Failed;

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);
        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<DataSetVersion> GetDataSetVersion(
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
            .Include(dsv => dsv.Imports.Where(i => i.InstanceId == instanceId))
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);
    }
}
