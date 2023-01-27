#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusContentStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishImmediateReleaseContentFunction
    {
        private readonly IContentService _contentService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublishingCompletionService _publishingCompletionService;

        public PublishImmediateReleaseContentFunction(
            IContentService contentService,
            IReleasePublishingStatusService releasePublishingStatusService,
            IPublishingCompletionService publishingCompletionService)
        {
            _contentService = contentService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _publishingCompletionService = publishingCompletionService;
        }

        /// <summary>
        /// Azure function which generates and publishes the content for a Release immediately.
        /// </summary>
        /// <remarks>
        /// Depends on the download files existing.
        /// Sets the published time on the Release which means it's considered as 'Live'.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishImmediateReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishImmediateReleaseContent(
            [QueueTrigger(PublishReleaseContentQueue)]
            PublishReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            try
            {
                await UpdateContentStage(message.ReleaseId, message.ReleaseStatusId, Started);
                await _contentService.UpdateContent(message.ReleaseId);
                await UpdateContentStage(message.ReleaseId, message.ReleaseStatusId, Complete);

                await _publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(
                    ListOf((message.ReleaseId, message.ReleaseStatusId)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {FunctionName}",
                    executionContext.FunctionName);

                await UpdateContentStage(message.ReleaseId, message.ReleaseStatusId, Failed,
                    new ReleasePublishingStatusLogMessage($"Exception publishing Release Content immediately: {e.Message}"));
            }

            logger.LogInformation("{0} completed", executionContext.FunctionName);
        }

        private async Task UpdateContentStage(
            Guid releaseId,
            Guid releaseStatusId,
            ReleasePublishingStatusContentStage state,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await _releasePublishingStatusService.UpdateContentStageAsync(releaseId, releaseStatusId, state, logMessage);
        }
    }
}
