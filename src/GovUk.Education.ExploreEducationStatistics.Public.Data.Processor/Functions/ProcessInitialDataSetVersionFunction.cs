using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessInitialDataSetVersionFunction(
    PublicDataDbContext publicDataDbContext,
    IDataSetMetaService dataSetMetaService,
    IDataDuckDbRepository dataDuckDbRepository,
    IParquetService parquetService,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    private readonly PublicDataDbContext _publicDataDbContext = publicDataDbContext;

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
            await context.CallActivity(nameof(CopyCsvFilesFunction.CopyCsvFiles), logger, context.InstanceId);
            await context.CallActivityExclusively(nameof(ImportMetadata), logger, context.InstanceId);
            await context.CallActivity(nameof(ImportData), logger, context.InstanceId);
            await context.CallActivity(nameof(WriteDataFiles), logger, context.InstanceId);
            await context.CallActivity(nameof(CompleteProcessing), logger, context.InstanceId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivity(nameof(HandleProcessingFailure), logger);
        }
    }

    [Function(nameof(ImportMetadata))]
    public async Task ImportMetadata(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.ImportingMetadata, cancellationToken);
        await dataSetMetaService.CreateDataSetVersionMeta(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }

    [Function(nameof(ImportData))]
    public async Task ImportData(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.ImportingData, cancellationToken);
        await dataDuckDbRepository.CreateDataTable(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }

    [Function(nameof(WriteDataFiles))]
    public async Task WriteDataFiles(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.WritingDataFiles, cancellationToken);
        await parquetService.WriteDataFiles(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }

    [Function(nameof(CompleteProcessing))]
    public async Task CompleteProcessing(
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
        await _publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    [Function(nameof(HandleProcessingFailure))]
    public async Task HandleProcessingFailure(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        dataSetVersion.Status = DataSetVersionStatus.Failed;
        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await _publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
