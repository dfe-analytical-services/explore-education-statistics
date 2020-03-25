using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class GenerateReleaseContentFunction
    {
        private readonly IContentService _contentService;
        private readonly IReleaseStatusService _releaseStatusService;

        public const string QueueName = "generate-release-content";

        public GenerateReleaseContentFunction(IContentService contentService,
            IReleaseStatusService releaseStatusService)
        {
            _contentService = contentService;
            _releaseStatusService = releaseStatusService;
        }

        [FunctionName("GenerateReleaseContent")]
        public async Task GenerateReleaseContent(
            [QueueTrigger(QueueName)] GenerateReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            await UpdateStage(message, Started);
            try
            {
                await _contentService.UpdateContentAsync(message.Releases.Select(tuple => tuple.ReleaseId));
                await UpdateStage(message, Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                await UpdateStage(message, Failed,
                    new ReleaseStatusLogMessage($"Exception in content stage: {e.Message}"));
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(GenerateReleaseContentMessage message, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                await _releaseStatusService.UpdateContentStageAsync(releaseId, releaseStatusId, stage, logMessage);
            }
        }
    }
}