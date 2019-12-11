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
        private readonly IReleaseStatusService _releaseStatusService;

        public const string QueueName = "publish-release-files";

        public PublishReleaseFilesFunction(IPublishingService publishingService,
            IReleaseStatusService releaseStatusService)
        {
            _publishingService = publishingService;
            _releaseStatusService = releaseStatusService;
        }

        [FunctionName("PublishReleaseFiles")]
        public async void PublishReleaseFiles(
            [QueueTrigger(QueueName)]
            PublishReleaseFilesMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            await UpdateStage(message, Started);
            _publishingService.PublishReleaseFiles(message).Wait();
            await UpdateStage(message, Complete);
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }
        
        private async Task UpdateStage(PublishReleaseFilesMessage message, Stage stage)
        {
            await _releaseStatusService.UpdateFilesStageAsync(message.ReleaseId, message.ReleaseStatusId, stage);
        }
    }
}