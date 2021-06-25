using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishMethodologyFilesFunction
    {
        private readonly IPublishingService _publishingService;

        public PublishMethodologyFilesFunction(IPublishingService publishingService)
        {
            _publishingService = publishingService;
        }

        [FunctionName("PublishMethodologyFiles")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishMethodologyFiles(
            [QueueTrigger(PublishMethodologyFilesQueue)]
            PublishMethodologyFilesMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message);

            await _publishingService.PublishMethodologyFiles(message.MethodologyId);

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }
    }
}
