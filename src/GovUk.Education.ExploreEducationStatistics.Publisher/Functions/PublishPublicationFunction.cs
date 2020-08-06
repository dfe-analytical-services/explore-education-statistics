using System;
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
        private readonly IPublicationService _publicationService;

        public PublishPublicationFunction(IContentService contentService,
            IPublicationService publicationService)
        {
            _contentService = contentService;
            _publicationService = publicationService;
        }

        [FunctionName("PublishPublication")]
        // ReSharper disable once UnusedMember.Global
        public void PublishPublication(
            [QueueTrigger(PublishPublicationQueue)]
            PublishPublicationMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");

            var context = new PublishContext(DateTime.UtcNow, false);

            _contentService.UpdatePublication(context, message.PublicationId);
            _publicationService.SetPublishedDate(message.PublicationId, context.Published);

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }
    }
}