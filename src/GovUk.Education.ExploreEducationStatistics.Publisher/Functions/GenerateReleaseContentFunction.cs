using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusContentStage;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Services.CronScheduleUtil;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class GenerateReleaseContentFunction
    {
        private readonly IContentService _contentService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public GenerateReleaseContentFunction(IContentService contentService,
            IReleasePublishingStatusService releasePublishingStatusService)
        {
            _contentService = contentService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which generates the content for a Release into a staging directory.
        /// </summary>
        /// <remarks>
        /// Depends on the download files existing.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("GenerateReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task GenerateStagedReleaseContent(
            [QueueTrigger(GenerateStagedReleaseContentQueue)] GenerateStagedReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message);
            await UpdateStage(message, Started);
            try
            {
                var context = new PublishContext(GetNextScheduledPublishingTime(), true);
                await _contentService.UpdateContent(context, message.Releases.Select(tuple => tuple.ReleaseId).ToArray());
                await UpdateStage(message, Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {FunctionName}",
                    executionContext.FunctionName);
                logger.LogError("{StackTrace}", e.StackTrace);
                await UpdateStage(message, Failed,
                    new ReleasePublishingStatusLogMessage($"Exception in content stage: {e.Message}"));
            }

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }

        private async Task UpdateStage(GenerateStagedReleaseContentMessage message, ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                await _releasePublishingStatusService.UpdateContentStageAsync(releaseId, releaseStatusId, stage, logMessage);
            }
        }
    }
}
