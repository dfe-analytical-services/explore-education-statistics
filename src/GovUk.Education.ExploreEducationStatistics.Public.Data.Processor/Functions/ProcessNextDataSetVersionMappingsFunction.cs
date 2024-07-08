using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessNextDataSetVersionFunction(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionMappingService mappingService) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(nameof(ProcessNextDataSetVersion))]
    public async Task ProcessNextDataSetVersion(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessDataSetVersionContext input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessNextDataSetVersion));

        logger.LogInformation(
            "Processing next data set version (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId);

        try
        {
            await context.CallActivity(ActivityNames.CopyCsvFiles, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.CreateMappings, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.ApplyAutoMappings, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.CompleteNextDataSetVersionMappingProcessing, logger,
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

    [Function(ActivityNames.CreateMappings)]
    public async Task CreateMappings(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.CreatingMappings, cancellationToken);
        await mappingService.CreateMappings(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }

    [Function(ActivityNames.ApplyAutoMappings)]
    public async Task ApplyAutoMappings(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.AutoMapping, cancellationToken);
        await mappingService.ApplyAutoMappings(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }

    [Function(ActivityNames.CompleteNextDataSetVersionMappingProcessing)]
    public async Task CompleteNextDataSetVersionMappingProcessing(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.Completing, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;
        dataSetVersion.Status = DataSetVersionStatus.Mapping;

        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
