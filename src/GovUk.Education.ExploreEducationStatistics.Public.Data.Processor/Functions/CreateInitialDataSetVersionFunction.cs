using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CreateInitialDataSetVersionFunction(
    ILogger<CreateInitialDataSetVersionFunction> logger,
    IDataSetService dataSetService,
    IValidator<InitialDataSetVersionCreateRequest> requestValidator)
{
    [Function(nameof(CreateInitialDataSetVersion))]
    public async Task<IActionResult> CreateInitialDataSetVersion(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = nameof(CreateInitialDataSetVersion))] [FromBody]
        InitialDataSetVersionCreateRequest request,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        // Identifier of the scheduled processing orchestration instance
        var instanceId = Guid.NewGuid();

        return await requestValidator.Validate(request, cancellationToken)
            .OnSuccess(() => dataSetService.CreateDataSetVersion(
                request,
                instanceId,
                cancellationToken: cancellationToken
            ))
            .OnSuccess(async tuple =>
            {
                await ProcessInitialDataSetVersion(
                    client,
                    dataSetVersionId: tuple.dataSetVersionId,
                    instanceId: instanceId,
                    cancellationToken);

                return new CreateInitialDataSetVersionResponseViewModel
                {
                    DataSetId = tuple.dataSetId,
                    DataSetVersionId = tuple.dataSetVersionId, 
                    InstanceId = instanceId
                };
            })
            .HandleFailuresOr(result => new OkObjectResult(result));
    }

    private async Task ProcessInitialDataSetVersion(
        DurableTaskClient client,
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        const string orchestratorName = nameof(ProcessInitialDataSetVersionFunction.ProcessInitialDataSetVersion);

        var input = new ProcessInitialDataSetVersionContext
        {
            DataSetVersionId = dataSetVersionId
        };

        var options = new StartOrchestrationOptions
        {
            InstanceId = instanceId.ToString()
        };

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
