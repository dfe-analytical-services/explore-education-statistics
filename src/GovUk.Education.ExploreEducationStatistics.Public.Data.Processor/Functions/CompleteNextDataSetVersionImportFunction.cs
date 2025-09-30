using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CompleteNextDataSetVersionImportFunction(
    ILogger<CompleteNextDataSetVersionImportFunction> logger,
    PublicDataDbContext publicDataDbContext,
    IValidator<NextDataSetVersionCompleteImportRequest> requestValidator,
    IDataSetVersionMappingService mappingService
)
{
    [Function(nameof(CompleteNextDataSetVersionImport))]
    public async Task<IActionResult> CompleteNextDataSetVersionImport(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = nameof(CompleteNextDataSetVersionImport)
        )]
        [FromBody]
            NextDataSetVersionCompleteImportRequest request,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken
    )
    {
        return await requestValidator
            .Validate(request, cancellationToken)
            .OnSuccess(_ =>
                mappingService.GetManualMappingVersionAndImport(request, cancellationToken)
            )
            .OnSuccess(async versionAndImport =>
            {
                var (nextVersion, importToContinue) = versionAndImport;

                importToContinue.InstanceId = Guid.NewGuid();
                publicDataDbContext.DataSetVersionImports.Update(importToContinue);

                nextVersion.Status = DataSetVersionStatus.Finalising;
                publicDataDbContext.DataSetVersions.Update(nextVersion);

                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                await ProcessCompletionOfNextDataSetVersionImport(
                    client,
                    dataSetVersionId: nextVersion.Id,
                    instanceId: importToContinue.InstanceId,
                    cancellationToken
                );

                return new ProcessDataSetVersionResponseViewModel
                {
                    DataSetId = nextVersion.DataSetId,
                    DataSetVersionId = nextVersion.Id,
                    InstanceId = importToContinue.InstanceId,
                };
            })
            .HandleFailuresOr(result => new OkObjectResult(result));
    }

    private async Task ProcessCompletionOfNextDataSetVersionImport(
        DurableTaskClient client,
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken
    )
    {
        const string orchestratorName = nameof(
            ProcessCompletionOfNextDataSetVersionOrchestration.ProcessCompletionOfNextDataSetVersionImport
        );

        var input = new ProcessDataSetVersionContext { DataSetVersionId = dataSetVersionId };

        var options = new StartOrchestrationOptions { InstanceId = instanceId.ToString() };

        logger.LogInformation(
            "Scheduling '{OrchestratorName}' (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            orchestratorName,
            instanceId,
            dataSetVersionId
        );

        await client.ScheduleNewOrchestrationInstanceAsync(
            orchestratorName,
            input,
            options,
            cancellationToken
        );
    }
}
