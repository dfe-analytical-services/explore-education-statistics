using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

public class ProcessorFunction(
    IDataSetService dataSetService,
    IValidator<ProcessorTriggerRequest> requestValidator)
{
    [Function(nameof(ProcessorFunction))]
    public async Task<List<string>> ProcessorOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessorOrchestration));

        logger.LogInformation("Running orchestration (Id={InstanceId})", context.InstanceId);

        var dataSetVersionId = context.GetInput<Guid>();
        List<string> output = [];

        try
        {
            // Other activity function calls to be added here to cover the following stages:
            // Move CSV files from Azure Blob Storage to Azure File Share
            // Create meta summary for the DataSetVersion
            // Import metadata to DuckDb
            // Import data to DuckDb
            // Export to Parquet files

            await context.CallActivityAsync(nameof(CompleteImportFunction.CompleteImport), dataSetVersionId);
            output.Add("Import complete");

            logger.LogInformation(
                "Activity '{ActivityName}' completed (Id={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(CompleteImportFunction.CompleteImport),
                context.InstanceId,
                dataSetVersionId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (Id={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                dataSetVersionId);

            await context.CallActivityAsync(nameof(HandleFailureFunction.HandleFailure), dataSetVersionId);
        }

        return output;
    }

    [Function(nameof(ProcessorTrigger))]
    public async Task<IActionResult> ProcessorTrigger(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrators/processor")] [FromBody]
        ProcessorTriggerRequest processorTriggerRequest,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        var logger = executionContext.GetLogger(nameof(ProcessorTrigger));

        var validationResult = await requestValidator.ValidateAsync(processorTriggerRequest, cancellationToken);
 
        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(validationResult.Errors.Select(failure => failure.ErrorMessage));
        }

        return await dataSetService
            .CreateDataSetVersion(processorTriggerRequest.ReleaseFileId, cancellationToken: cancellationToken)
            .OnSuccess(async dataSetVersionId =>
            {
                var instanceId = await ScheduleNewOrchestrationInstance(client, dataSetVersionId, cancellationToken);

                logger.LogInformation("Scheduled orchestration (Id={InstanceId})", instanceId);

                return new ProcessorTriggerResponseViewModel
                {
                    DataSetVersionId = dataSetVersionId, InstanceId = instanceId
                };
            })
            .HandleFailuresOr(result => new OkObjectResult(result));
    }

    private static async Task<Guid> ScheduleNewOrchestrationInstance(
        DurableTaskClient client,
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        var instanceId = Guid.NewGuid();
        await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ProcessorFunction),
            dataSetVersionId,
            new StartOrchestrationOptions
            {
                InstanceId = instanceId.ToString()
            },
            cancellationToken);

        return instanceId;
    }
}
