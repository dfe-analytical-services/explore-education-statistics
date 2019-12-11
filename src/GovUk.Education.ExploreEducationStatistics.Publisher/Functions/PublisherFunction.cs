using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseInfoTaskStatus;

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
        public async void GenerateReleaseContent(
            [QueueTrigger("generate-release-content")]
            GenerateReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            _contentCacheGenerationService.GenerateReleaseContent(message).Wait();
            // TODO EES-861 ReleaseId is currently optional because we allow using the message to initiate a full content refresh
            if (message.ReleaseId.HasValue && message.ReleaseInfoId.HasValue)
            {
                await _releaseInfoService.UpdateContentStatusAsync(message.ReleaseId.Value, message.ReleaseInfoId.Value,
                    Complete);
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        [FunctionName("PublishReleaseContent")]
        public void PublishReleaseContent([TimerTrigger("0 30 9 * * *")] TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered at: {DateTime.Now}");
            // TODO EES-865 Move content
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
            _publishingService.PublishReleaseDataFiles(message).Wait();
            await _releaseInfoService.UpdateFilesStatusAsync(message.ReleaseId, message.ReleaseInfoId, Complete);
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        [FunctionName("PublishReleaseData")]
        public async void PublishReleaseData(
            [QueueTrigger("publish-release-data")] PublishReleaseDataMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            // TODO EES-866 Run the importer or copy the data from the statistics database
            // TODO EES-866 to the publicly available statistics database
            await _releaseInfoService.UpdateDataStatusAsync(message.ReleaseId, message.ReleaseInfoId, Failed);
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }
    }
}