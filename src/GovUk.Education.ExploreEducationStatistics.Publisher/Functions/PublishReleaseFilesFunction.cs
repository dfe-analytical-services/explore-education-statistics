using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseFilesFunction
    {
        private readonly IPublishingService _publishingService;
        private readonly IQueueService _queueService;
        private readonly IReleaseStatusService _releaseStatusService;

        public const string QueueName = "publish-release-files";

        public PublishReleaseFilesFunction(IPublishingService publishingService,
            IQueueService queueService,
            IReleaseStatusService releaseStatusService)
        {
            _publishingService = publishingService;
            _queueService = queueService;
            _releaseStatusService = releaseStatusService;
        }

        [FunctionName("PublishReleaseFiles")]
        public async Task PublishReleaseFiles(
            [QueueTrigger(QueueName)] PublishReleaseFilesMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");

            var published = new List<(Guid ReleaseId, Guid ReleaseStatusId)>();
            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                await UpdateFilesStage(releaseId, releaseStatusId, Started);
                try
                {
                    _publishingService.PublishReleaseFilesAsync(releaseId).Wait();
                    published.Add((releaseId, releaseStatusId));
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                    await UpdateFilesStage(releaseId, releaseStatusId, Failed,
                        new ReleaseStatusLogMessage($"Exception in files stage: {e.Message}"));
                }
            }

            await _queueService.QueueGenerateReleaseContentMessageAsync(published);

            foreach (var (releaseId, releaseStatusId) in published)
            {
                await UpdateFilesStage(releaseId, releaseStatusId, Complete);
                await UpdateContentStage(releaseId, releaseStatusId, Queued);
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateFilesStage(Guid releaseId, Guid releaseStatusId, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await _releaseStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId, stage, logMessage);
        }

        private async Task UpdateContentStage(Guid releaseId, Guid releaseStatusId, Stage stage)
        {
            await _releaseStatusService.UpdateContentStageAsync(releaseId, releaseStatusId, stage);
        }
    }
}