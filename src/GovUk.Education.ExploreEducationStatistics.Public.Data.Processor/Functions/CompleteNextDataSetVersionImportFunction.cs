using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CompleteNextDataSetVersionProcessingFunction(
    ILogger<CompleteNextDataSetVersionProcessingFunction> logger,
    PublicDataDbContext publicDataDbContext,
    IValidator<NextDataSetVersionCompleteImportRequest> requestValidator)
{
    [Function(nameof(CompleteNextDataSetVersionImport))]
    public async Task<IActionResult> CompleteNextDataSetVersionImport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(CompleteNextDataSetVersionImport))] [FromBody]
        NextDataSetVersionCompleteImportRequest request,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        var instanceId = Guid.NewGuid();

        return await requestValidator.Validate(request, cancellationToken)
            .OnSuccess(_ => publicDataDbContext
                .DataSetVersions
                .AsNoTracking()
                .SingleOrNotFoundAsync(
                    dataSetVersion => dataSetVersion.Id == request.DataSetVersionId, 
                    cancellationToken))
            .OnSuccess(async nextDataSetVersion =>
            {
                await ProcessCompletionOfNextDataSetVersionImport(
                    client,
                    dataSetVersionId: nextDataSetVersion.Id,
                    instanceId: instanceId,
                    cancellationToken);

                return new ProcessDataSetVersionResponseViewModel
                {
                    DataSetId = nextDataSetVersion.DataSetId,
                    DataSetVersionId = nextDataSetVersion.Id,
                    InstanceId = instanceId
                };
            })
            .HandleFailuresOr(result => new OkObjectResult(result));
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
}
