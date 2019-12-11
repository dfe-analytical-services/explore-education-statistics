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
        private readonly IContentCacheGenerationService _contentCacheGenerationService;
        private readonly IReleaseStatusService _releaseStatusService;

        public const string QueueName = "generate-release-content";

        public GenerateReleaseContentFunction(IContentCacheGenerationService contentCacheGenerationService,
            IReleaseStatusService releaseStatusService)
        {
            _contentCacheGenerationService = contentCacheGenerationService;
            _releaseStatusService = releaseStatusService;
        }

        [FunctionName("GenerateReleaseContent")]
        public async void GenerateReleaseContent(
            [QueueTrigger(QueueName)] GenerateReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            await UpdateStage(message, Started);
            _contentCacheGenerationService.GenerateReleaseContent(message).Wait();
            await UpdateStage(message, Complete);

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(GenerateReleaseContentMessage message, Stage stage)
        {
            // TODO EES-861 ReleaseId is currently optional because we allow using the message to initiate a full content refresh
            if (message.ReleaseId.HasValue && message.ReleaseStatusId.HasValue)
            {
                await _releaseStatusService.UpdateContentStageAsync(message.ReleaseId.Value,
                    message.ReleaseStatusId.Value, stage);
            }
        }
    }
}