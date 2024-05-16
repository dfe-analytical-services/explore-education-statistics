using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessInitialDataSetVersionFunction(
    PublicDataDbContext publicDataDbContext,
    IDataSetMetaService dataSetMetaService,
    IDataRepository dataRepository,
    IParquetService parquetService,
    IDataSetVersionPathResolver dataSetVersionPathResolver)
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
            await context.CallActivityAsync(nameof(CopyCsvFilesFunction.CopyCsvFiles),
                input.DataSetVersionId);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(CopyCsvFilesFunction.CopyCsvFiles),
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivityAsync(nameof(ImportMetadata), input.DataSetVersionId);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(ImportMetadata),
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivityAsync(nameof(ImportData), input.DataSetVersionId);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(ImportData),
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivityAsync(nameof(ExportData), input.DataSetVersionId);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(ExportData),
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivityAsync(nameof(CompleteProcessing), input.DataSetVersionId);

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

            await context.CallActivityAsync(nameof(HandleProcessingFailure), input.DataSetVersionId);
        }
    }

    [Function(nameof(ImportMetadata))]
    public async Task ImportMetadata(
        [ActivityTrigger] Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: dataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);

        dataSetVersionImport.Stage = DataSetVersionImportStage.ImportingMetadata;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        await dataSetMetaService.CreateDataSetVersionMeta(dataSetVersion, cancellationToken);
    }

    [Function(nameof(ImportData))]
    public async Task ImportData(
        [ActivityTrigger] Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: dataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);

        dataSetVersionImport.Stage = DataSetVersionImportStage.ImportingData;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        await using var duckDb =
            DuckDbConnection.FileConnection(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
        duckDb.Open();

        await dataRepository.CreateDataTable(duckDb, dataSetVersion, cancellationToken);
    }

    [Function(nameof(ExportData))]
    public async Task ExportData(
        [ActivityTrigger] Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: dataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);

        dataSetVersionImport.Stage = DataSetVersionImportStage.ExportingData;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        await parquetService.WriteData(dataSetVersion, cancellationToken);
    }

    [Function(nameof(CompleteProcessing))]
    public async Task CompleteProcessing(
        [ActivityTrigger] Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: dataSetVersionId,
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
        [ActivityTrigger] Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: dataSetVersionId,
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
