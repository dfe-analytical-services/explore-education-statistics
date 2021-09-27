using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusDataStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class DataFactoryPipelineStatusFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public DataFactoryPipelineStatusFunction(IQueueService queueService, IReleasePublishingStatusService releasePublishingStatusService)
        {
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which updates the stage of the statistics data task depending on the result of the ADF Pipeline.
        /// This is triggered when the ADF Pipeline completes regardless of whether it was successful or not.
        /// </summary>
        /// <remarks>
        /// Triggers publishing content for the Release if publishing is immediate.
        /// </remarks>
        /// <param name="req"></param>
        /// <param name="logger"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [FunctionName("DataFactoryPipelineStatus")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "datafactory/pipeline/status/")]
            HttpRequest req,
            ILogger logger,
            ExecutionContext executionContext)
        {
            logger.LogInformation("{0} triggered",
                executionContext.FunctionName);
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<PipelineResponse>(requestBody);

            if (response.Status == "Complete")
            {
                await _releasePublishingStatusService.UpdateDataStageAsync(response.ReleaseId, response.ReleaseStatusId,
                    Complete);

                if (await _releasePublishingStatusService.IsImmediate(response.ReleaseId, response.ReleaseStatusId))
                {
                    await _queueService.QueuePublishReleaseContentMessageAsync(response.ReleaseId,
                        response.ReleaseStatusId);
                }
            }
            else
            {
                logger.LogError("ADF pipeline failed: {0}", response);
                await _releasePublishingStatusService.UpdateDataStageAsync(response.ReleaseId, response.ReleaseStatusId, Failed,
                    new ReleasePublishingStatusLogMessage(
                        $"Exception in data stage (ADF pipeline triggered: {response.PipelineTriggerTime}): {response.ErrorMessage}"));
            }

            logger.LogInformation("{0} completed", executionContext.FunctionName);

            return response.Status != null
                ? (ActionResult) new OkObjectResult($"status, {response.Status}")
                : new BadRequestObjectResult("No status was passed in the request body");
        }
    }

    internal class PipelineResponse
    {
        public string DataFactoryName { get; set; }
        public string ErrorMessage { get; set; }
        public string PipelineName { get; set; }
        public string PipelineTriggerTime { get; set; }
        public Guid ReleaseId { get; set; }
        public Guid ReleaseStatusId { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(DataFactoryName)}: {DataFactoryName}, {nameof(ErrorMessage)}: {ErrorMessage}, {nameof(PipelineName)}: {PipelineName}, {nameof(PipelineTriggerTime)}: {PipelineTriggerTime}, {nameof(ReleaseId)}: {ReleaseId}, {nameof(ReleaseStatusId)}: {ReleaseStatusId}, {nameof(Status)}: {Status}";
        }
    }
}
