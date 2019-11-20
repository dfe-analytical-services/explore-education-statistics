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
        public void RebuildContentCacheTrees(
            [QueueTrigger("content-cache")]
            string message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");

            // TODO: switch actions based on message content, for now full rebuild
            switch (message)
            {
                default:
                    _contentCacheGenerationService.CleanAndRebuildFullCache().Wait();
                    break;
            }

            logger.LogInformation($"{GetType().Name} function completed");
        }
    }
}