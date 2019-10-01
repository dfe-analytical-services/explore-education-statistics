using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class Publisher
    {
        private readonly IPublishingService _publishingService;

        public Publisher(IPublishingService publishingService)
        {
            _publishingService = publishingService;
        }

        [FunctionName("Publisher")]
        public async void PublishReleaseData(
            [QueueTrigger("publish-release-data", Connection = "")]
            PublishReleaseDataMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");
            await _publishingService.PublishReleaseData(message);
            logger.LogInformation($"{GetType().Name} function completed");
        }
       
    }
}