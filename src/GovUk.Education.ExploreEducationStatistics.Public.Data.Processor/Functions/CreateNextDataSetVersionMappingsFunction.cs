using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CreateNextDataSetVersionMappingsFunction(
    ILogger<CreateNextDataSetVersionMappingsFunction> logger,
    IDataSetVersionService dataSetVersionService,
    IValidator<NextDataSetVersionMappingsCreateRequest> requestValidator)
{
    [Function(nameof(CreateNextDataSetVersionMappings))]
    public async Task<IActionResult> CreateNextDataSetVersionMappings(
        [HttpTrigger(
            AuthorizationLevel.Anonymous, "post",
            Route = nameof(CreateNextDataSetVersionMappings))]
        [FromBody]
        NextDataSetVersionMappingsCreateRequest request,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        // Identifier of the scheduled processing orchestration instance
        var instanceId = Guid.NewGuid();
        
        return await requestValidator.Validate(request, cancellationToken)
            .OnSuccess(() =>
                 dataSetVersionService.CreateNextVersion(
                    dataSetId: request.DataSetId,
                    releaseFileId: request.ReleaseFileId,
                    instanceId,
                    request.DataSetVersionToReplaceId,
                    cancellationToken: cancellationToken
                ))
            .OnSuccess(async dataSetVersionId =>
            {
                await ProcessNextDataSetVersion(
                    client,
                    dataSetVersionId: dataSetVersionId,
                    instanceId: instanceId,
                    cancellationToken);

                return new ProcessDataSetVersionResponseViewModel
                {
                    DataSetId = request.DataSetId,
                    DataSetVersionId = dataSetVersionId,
                    InstanceId = instanceId
                };
            })
            .HandleFailuresOr(result => new OkObjectResult(result));
    }

    private async Task ProcessNextDataSetVersion(
        DurableTaskClient client,
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        const string orchestratorName =
            nameof(ProcessNextDataSetVersionMappingsFunctionOrchestration.ProcessNextDataSetVersionMappings);

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
}
