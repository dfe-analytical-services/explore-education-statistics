using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublisherFunction
    {
        private readonly IContentCacheGenerationService _contentCacheGenerationService;
        private readonly IPublishingService _publishingService;

        public PublisherFunction(IContentCacheGenerationService contentCacheGenerationService,
            IPublishingService publishingService)
        {
            _contentCacheGenerationService = contentCacheGenerationService;
            _publishingService = publishingService;
        }

        [FunctionName("PublishReleaseContent")]
        public void PublishReleaseContent(
            [QueueTrigger("publish-release-content")]
            PublishReleaseContentMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");
            _contentCacheGenerationService.PublishReleaseContent(message).Wait();
            logger.LogInformation($"{GetType().Name} function completed");
        }

        [FunctionName("PublishReleaseDataFiles")]
        public async void PublishReleaseDataFiles(
            [QueueTrigger("publish-release-data-files")]
            PublishReleaseDataFilesMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");
            await _publishingService.PublishReleaseDataFiles(message);
            logger.LogInformation($"{GetType().Name} function completed");
        }

        [FunctionName("PublishReleaseData")]
        public async void PublishReleaseData(
            [QueueTrigger("publish-release-data")] PublishReleaseDataMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");
            // TODO EES-866 Run the importer or copy the data from the statistics database
            // TODO EES-866 to the publicly available statistics database
            logger.LogInformation($"{GetType().Name} function completed");
        }
    }
}