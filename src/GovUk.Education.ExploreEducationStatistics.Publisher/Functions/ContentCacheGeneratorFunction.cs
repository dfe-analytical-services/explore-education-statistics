using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class ContentCacheGeneration
    {
        private readonly IContentCacheGenerationService _contentCacheGenerationService;

        public ContentCacheGeneration(IContentCacheGenerationService contentCacheGenerationService)
        {
            _contentCacheGenerationService = contentCacheGenerationService;
        }

        [FunctionName("RebuildContentCache")]
        public async void RebuildContentCacheTrees(
            [QueueTrigger("content-cache", Connection = "")]
            // TODO: create the cache message object
            string message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");

            // TODO: switch actions based on message content, for now full rebuild
            await _contentCacheGenerationService.CleanAndRebuildFullCache();

            logger.LogInformation($"{GetType().Name} function completed");
        }
    }
}