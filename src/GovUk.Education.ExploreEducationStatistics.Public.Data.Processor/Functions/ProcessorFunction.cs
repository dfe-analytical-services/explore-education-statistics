using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessorFunction(
    ContentDbContext contentDbContext,
    IDataSetService dataSetService,
    IDataSetVersionRepository dataSetVersionRepository
)
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
            // Import meta data to DuckDb
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

            await dataSetVersionRepository.UpdateStatus(dataSetVersionId, DataSetVersionStatus.Failed);
        }

        return output;
    }

    [Function(nameof(ProcessorTrigger))]
    public async Task<IActionResult> ProcessorTrigger(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrators/processor")]
        HttpRequest req,
        [Microsoft.Azure.Functions.Worker.Http.FromBody]
        ProcessorTriggerRequest processorTriggerRequest,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        var logger = executionContext.GetLogger(nameof(ProcessorTrigger));

        //await ValidateRequest(processorTriggerRequest);

        var dataSetVersionId = await dataSetService.CreateDataSetVersion(
            releaseFileId: processorTriggerRequest.ReleaseFileId,
            cancellationToken: cancellationToken);

        var instanceId = Guid.NewGuid();
        await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ProcessorFunction),
            dataSetVersionId,
            new StartOrchestrationOptions
            {
                InstanceId = instanceId.ToString()
            },
            cancellationToken);

        logger.LogInformation("Started orchestration (Id={InstanceId})", instanceId);

        return new OkObjectResult(new ProcessorTriggerResponseViewModel
        {
            DataSetVersionId = dataSetVersionId, InstanceId = instanceId
        });
    }

    // TODO Tidy this up / move it into a Validator
    private async Task ValidateRequest(ProcessorTriggerRequest request)
    {
        var releaseFileInfo = await contentDbContext.ReleaseFiles
            .Where(rf => rf.Id == request.ReleaseFileId)
            .Select(rf => new
            {
                rf.ReleaseVersionId,
                rf.File.SubjectId,
                rf.File.Type,
                rf.ReleaseVersion.PublicationId,
                ReleaseVersionApprovalStatus = rf.ReleaseVersion.ApprovalStatus
            })
            .SingleOrDefaultAsync();

        // ReleaseFile must exist
        if (releaseFileInfo == null)
        {
            throw new ArgumentException($"ReleaseFile does not exist (ReleaseFileId={request.ReleaseFileId})");
        }

        // ReleaseFile must relate to a File of type Data
        if (releaseFileInfo.Type != FileType.Data)
        {
            throw new ArgumentException($"ReleaseFile is not of type Data (ReleaseFileId={request.ReleaseFileId})");
        }

        // ReleaseFile must relate to a ReleaseVersion in Draft approval status
        if (releaseFileInfo.ReleaseVersionApprovalStatus != ReleaseApprovalStatus.Draft)
        {
            throw new ArgumentException(
                $"ReleaseFile does not relate to a ReleaseVersion in Draft status (ReleaseFileId={request.ReleaseFileId})");
        }

        // There must be a ReleaseFile related to the same ReleaseVersion and Subject with File of type Metadata
        if (!await contentDbContext.ReleaseFiles
                .Where(rf => rf.ReleaseVersionId == releaseFileInfo.ReleaseVersionId)
                .Where(rf => rf.File.SubjectId == releaseFileInfo.SubjectId)
                .Where(rf => rf.File.Type == FileType.Metadata)
                .AnyAsync())
        {
            throw new ArgumentException(
                $"ReleaseFile does not have a corresponding ReleaseFile of type Metadata (ReleaseFileId={request.ReleaseFileId})");
        }
    }
}
