using System;
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

        [FunctionName("GenerateReleaseContent")]
        public void GenerateReleaseContent(
            [QueueTrigger("generate-release-content")]
            GenerateReleaseContentMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().FullName} function triggered: {message}");
            _contentCacheGenerationService.GenerateReleaseContent(message).Wait();
            logger.LogInformation($"{GetType().FullName} function completed");
        }

        [FunctionName("PublishReleaseContent")]
        public void PublishReleaseContent([TimerTrigger("0 30 9 * * *")] TimerInfo timer,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().FullName} function triggered at: {DateTime.Now}");
            // TODO EES-865 Move content daily at 09:30
            logger.LogInformation(
                $"{GetType().FullName} function completed. Next occurrence at: {timer.FormatNextOccurrences(1)}");
        }
        
        [FunctionName("PublishReleaseDataFiles")]
        public async void PublishReleaseDataFiles(
            [QueueTrigger("publish-release-data-files")]
            PublishReleaseDataFilesMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().FullName} function triggered: {message}");
            await _publishingService.PublishReleaseDataFiles(message);
            logger.LogInformation($"{GetType().FullName} function completed");
        }

        [FunctionName("PublishReleaseData")]
        public void PublishReleaseData(
            [QueueTrigger("publish-release-data")] PublishReleaseDataMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().FullName} function triggered: {message}");
            // TODO EES-866 Run the importer or copy the data from the statistics database
            // TODO EES-866 to the publicly available statistics database
            logger.LogInformation($"{GetType().FullName} function completed");
        }
    }
}