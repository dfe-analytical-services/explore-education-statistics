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
        private readonly IReleaseInfoService _releaseInfoService;

        public PublisherFunction(IContentCacheGenerationService contentCacheGenerationService,
            IPublishingService publishingService,
            IReleaseInfoService releaseInfoService)
        {
            _contentCacheGenerationService = contentCacheGenerationService;
            _publishingService = publishingService;
            _releaseInfoService = releaseInfoService;
        }

        [FunctionName("GenerateReleaseContent")]
        public void GenerateReleaseContent(
            [QueueTrigger("generate-release-content")]
            GenerateReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            _contentCacheGenerationService.GenerateReleaseContent(message).Wait();
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        [FunctionName("PublishReleaseContent")]
        public void PublishReleaseContent([TimerTrigger("0 30 9 * * *")] TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered at: {DateTime.Now}");
            // TODO EES-865 Move content daily at 09:30
            logger.LogInformation(
                $"{executionContext.FunctionName} completed. {timer.FormatNextOccurrences(1)}");
        }

        [FunctionName("PublishReleaseDataFiles")]
        public async void PublishReleaseDataFiles(
            [QueueTrigger("publish-release-data-files")]
            PublishReleaseDataFilesMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            await _publishingService.PublishReleaseDataFiles(message);
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        [FunctionName("PublishReleaseData")]
        public void PublishReleaseData(
            [QueueTrigger("publish-release-data")] PublishReleaseDataMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            // TODO EES-866 Run the importer or copy the data from the statistics database
            // TODO EES-866 to the publicly available statistics database
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }
    }
}