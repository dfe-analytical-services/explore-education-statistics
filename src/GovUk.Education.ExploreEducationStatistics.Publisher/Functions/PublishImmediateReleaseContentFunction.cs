using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishImmediateReleaseContentFunction(
        ILogger<PublishImmediateReleaseContentFunction> logger,
        IContentService contentService,
        IReleasePublishingStatusService releasePublishingStatusService,
        IPublishingCompletionService publishingCompletionService)
    {
        /// <summary>
        /// Azure function which generates and publishes the content for a Release immediately.
        /// </summary>
        /// <remarks>
        /// Depends on the download files existing.
        /// Sets the published time on the Release which means it's considered as 'Live'.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Function("PublishImmediateReleaseContent")]
        public async Task PublishImmediateReleaseContent(
            [QueueTrigger(PublishReleaseContentQueue)] PublishReleaseContentMessage message,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            try
            {
                await UpdateContentStage(message.ReleasePublishingKeyOld, ReleasePublishingStatusContentStage.Started);
                await contentService.UpdateContent(message.ReleasePublishingKeyOld.ReleaseVersionId);
                await UpdateContentStage(message.ReleasePublishingKeyOld, ReleasePublishingStatusContentStage.Complete);
                await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(new[]
                {
                    message.ReleasePublishingKeyOld
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {FunctionName}", context.FunctionDefinition.Name);

                await UpdateContentStage(
                    message.ReleasePublishingKeyOld,
                    ReleasePublishingStatusContentStage.Failed,
                    new ReleasePublishingStatusLogMessage(
                        $"Exception publishing Release Content immediately: {e.Message}"));
            }

            logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
        }

        private async Task UpdateContentStage(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusContentStage state,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await releasePublishingStatusService.UpdateContentStage(
                releasePublishingKeyOld,
                state,
                logMessage);
        }
    }
}
