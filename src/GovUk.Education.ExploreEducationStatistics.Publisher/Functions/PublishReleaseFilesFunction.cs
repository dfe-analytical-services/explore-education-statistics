using System;
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
            await UpdateStage(message, Started);
            try
            {
                _publishingService.PublishReleaseFilesAsync(message.ReleaseId).Wait();
                await _queueService.QueueGenerateReleaseContentMessageAsync(message.ReleaseId, message.ReleaseStatusId);
                await UpdateContentStage(message, Queued);
                await UpdateStage(message, Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                await UpdateStage(message, Failed);
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(PublishReleaseFilesMessage message, Stage stage)
        {
            await _releaseStatusService.UpdateFilesStageAsync(message.ReleaseId, message.ReleaseStatusId, stage);
        }
        
        private async Task UpdateContentStage(PublishReleaseFilesMessage message, Stage stage)
        {
            await _releaseStatusService.UpdateContentStageAsync(message.ReleaseId, message.ReleaseStatusId, stage);
        }
    }
}