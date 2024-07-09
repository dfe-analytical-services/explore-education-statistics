using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using ValidationMessages =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CompleteNextDataSetVersionImportFunction(
    ILogger<CompleteNextDataSetVersionImportFunction> logger,
    PublicDataDbContext publicDataDbContext,
    IValidator<DataSetVersionProcessRequest> requestValidator)
{
    [Function(nameof(CompleteNextDataSetVersionImport))]
    public async Task<IActionResult> CompleteNextDataSetVersionImport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(CompleteNextDataSetVersionImport))] [FromBody]
        DataSetVersionProcessRequest request,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        return await requestValidator
            .Validate(request, cancellationToken)
            .OnSuccess(_ => GetNextDataSetVersionInMappingStatus(request, cancellationToken))
            .OnSuccessDo(_ => GetCompletedDataSetVersionMapping(request, cancellationToken))
            .OnSuccessCombineWith(nextDataSetVersion =>
                GetImportInManualMappingStage(request, nextDataSetVersion))
            .OnSuccess(async versionAndImport =>
            {
                var (nextVersion, importToContinue) = versionAndImport;

                importToContinue.InstanceId = Guid.NewGuid();
                publicDataDbContext.DataSetVersionImports.Update(importToContinue);
                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                await ProcessCompletionOfNextDataSetVersionImport(
                    client,
                    dataSetVersionId: nextVersion.Id,
                    instanceId: importToContinue.InstanceId,
                    cancellationToken);

                return new ProcessDataSetVersionResponseViewModel
                {
                    DataSetId = nextVersion.DataSetId,
                    DataSetVersionId = nextVersion.Id,
                    InstanceId = importToContinue.InstanceId
                };
            })
            .HandleFailuresOr(result => new OkObjectResult(result));
    }

    private static Either<ActionResult, DataSetVersionImport> GetImportInManualMappingStage(
        DataSetVersionProcessRequest request,
        DataSetVersion nextDataSetVersion)
    {
        var importToContinue = nextDataSetVersion
            .Imports
            .SingleOrDefault(import => import.DataSetVersionId == nextDataSetVersion.Id
                                       && import.Stage == DataSetVersionImportStage.ManualMapping);

        return importToContinue is null
            ? CreateDataSetVersionIdError(
                message: ValidationMessages.ImportInManualMappingStateNotFound,
                dataSetVersionId: request.DataSetVersionId)
            : importToContinue;
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetNextDataSetVersionInMappingStatus(
        DataSetVersionProcessRequest request,
        CancellationToken cancellationToken)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .AsNoTracking()
            .Include(dataSetVersion => dataSetVersion.Imports)
            .SingleOrDefaultAsync(
                dataSetVersion => dataSetVersion.Id == request.DataSetVersionId,
                cancellationToken);

        if (nextVersion is null)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionNotFound,
                dataSetVersionId: request.DataSetVersionId);
        }

        if (nextVersion.Status != DataSetVersionStatus.Mapping)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionNotInMappingStatus,
                dataSetVersionId: request.DataSetVersionId);
        }

        return nextVersion;
    }

    private async Task<Either<ActionResult, DataSetVersionMapping>> GetCompletedDataSetVersionMapping(
        DataSetVersionProcessRequest request,
        CancellationToken cancellationToken)
    {
        var mapping = await publicDataDbContext
            .DataSetVersionMappings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                mapping => mapping.TargetDataSetVersionId == request.DataSetVersionId,
                cancellationToken);

        if (mapping is null)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionMappingNotFound,
                dataSetVersionId: request.DataSetVersionId);
        }

        if (!mapping.FilterMappingsComplete || !mapping.LocationMappingsComplete)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionMappingsNotComplete,
                dataSetVersionId: request.DataSetVersionId);
        }

        return mapping;
    }

    private async Task ProcessCompletionOfNextDataSetVersionImport(
        DurableTaskClient client,
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        const string orchestratorName =
            nameof(ProcessCompletionOfNextDataSetVersionFunction.CompleteNextDataSetVersionImportProcessing);

        var input = new ProcessDataSetVersionContext { DataSetVersionId = dataSetVersionId };

        var options = new StartOrchestrationOptions { InstanceId = instanceId.ToString() };

        logger.LogInformation(
            "Scheduling '{OrchestratorName}' (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            orchestratorName,
            instanceId,
            dataSetVersionId);

        await client.ScheduleNewOrchestrationInstanceAsync(
            orchestratorName,
            input,
            options,
            cancellationToken);
    }

    private static BadRequestObjectResult CreateDataSetVersionIdError(
        LocalizableMessage message,
        Guid dataSetVersionId)
    {
        return ValidationUtils.ValidationResult(new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = nameof(DataSetVersionProcessRequest.DataSetVersionId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(dataSetVersionId)
        });
    }
}
