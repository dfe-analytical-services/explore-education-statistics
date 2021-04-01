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
    public class PublishMethodologyFunction
    {
        private readonly IContentService _contentService;
        private readonly IPublishingService _publishingService;

        public PublishMethodologyFunction(IContentService contentService,
            IPublishingService publishingService)
        {
            _contentService = contentService;
            _publishingService = publishingService;
        }

        [FunctionName("PublishMethodology")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishMethodology(
            [QueueTrigger(PublishMethodologyQueue)]
            PublishMethodologyMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message);

            var context = new PublishContext(DateTime.UtcNow, false);

            await _contentService.UpdateMethodology(context, message.MethodologyId);
            await _publishingService.PublishMethodologyFiles(message.MethodologyId);

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }
    }
}
