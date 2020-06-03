using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusContentStage;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Services.CronScheduleUtil;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class GenerateReleaseContentFunction
    {
        private readonly IContentService _contentService;
        private readonly IReleaseStatusService _releaseStatusService;

        public GenerateReleaseContentFunction(IContentService contentService,
            IReleaseStatusService releaseStatusService)
        {
            _contentService = contentService;
            _releaseStatusService = releaseStatusService;
        }

        /**
         * Azure function which generates the content for a Release into a staging directory.
         * Depends on the download files existing.
         */
        [FunctionName("GenerateReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task GenerateReleaseContent(
            [QueueTrigger(GenerateReleaseContentQueue)] GenerateReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            await UpdateStage(message, Started);
            try
            {
                var context = new PublishContext(GetNextScheduledPublishingTime(), true);
                await _contentService.UpdateContentAsync(message.Releases.Select(tuple => tuple.ReleaseId), context);
                await UpdateStage(message, Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                logger.LogError(e.StackTrace);
                await UpdateStage(message, Failed,
                    new ReleaseStatusLogMessage($"Exception in content stage: {e.Message}"));
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(GenerateReleaseContentMessage message, ReleaseStatusContentStage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                await _releaseStatusService.UpdateContentStageAsync(releaseId, releaseStatusId, stage, logMessage);
            }
        }
    }
}