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
    public class PublishPublicationFunction
    {
        private readonly IContentService _contentService;

        public PublishPublicationFunction(IContentService contentService)
        {
            _contentService = contentService;
        }

        [FunctionName("PublishPublication")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishPublication(
            [QueueTrigger(PublishPublicationQueue)]
            PublishPublicationMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message);

            var context = new PublishContext(DateTime.UtcNow, false);

            await _contentService.UpdatePublication(context, message.PublicationId);

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }
    }
}
