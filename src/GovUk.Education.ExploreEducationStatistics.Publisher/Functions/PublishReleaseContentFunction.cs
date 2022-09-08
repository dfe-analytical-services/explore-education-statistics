#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseContentFunction
    {
        private readonly IContentService _contentService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublishingCompletionService _publishingCompletionService;

        public PublishReleaseContentFunction(
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
        [FunctionName("PublishReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleaseContent(
            [QueueTrigger(PublishReleaseContentQueue)]
            PublishReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Started);

            var context = new PublishContext(DateTime.UtcNow, false);

            try
            {
                await _contentService.UpdateContent(context, message.ReleaseId);
                await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Complete);
                await _publishingCompletionService.CompletePublishingIfAllStagesComplete(
                    message.ReleaseId, 
                    message.ReleaseStatusId,
                    context.Published);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {FunctionName}",
                    executionContext.FunctionName);
                logger.LogError("{StackTrace}", e.StackTrace);

                await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Failed,
                    new ReleasePublishingStatusLogMessage($"Exception publishing release immediately: {e.Message}"));
            }

            logger.LogInformation("{0} completed", executionContext.FunctionName);
        }

        private async Task UpdateStage(Guid releaseId, Guid releaseStatusId, State state,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            switch (state)
            {
                case State.Started:
                    await _releasePublishingStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleasePublishingStatusPublishingStage.Started,
                        content: ReleasePublishingStatusContentStage.Started);
                    break;
                case State.Complete:
                    await _releasePublishingStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleasePublishingStatusPublishingStage.Complete,
                        content: ReleasePublishingStatusContentStage.Complete);
                    break;
                case State.Failed:
                    await _releasePublishingStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleasePublishingStatusPublishingStage.Failed,
                        content: ReleasePublishingStatusContentStage.Failed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private enum State
        {
            Started,
            Complete,
            Failed
        }
    }
}
