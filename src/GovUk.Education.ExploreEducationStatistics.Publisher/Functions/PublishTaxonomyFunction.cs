using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishTaxonomyFunction
    {
        private readonly IContentService _contentService;

        public PublishTaxonomyFunction(IContentService contentService)
        {
            _contentService = contentService;
        }

        [FunctionName("PublishTaxonomy")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishTaxonomy(
            [QueueTrigger(PublishTaxonomyQueue)]
            PublishTaxonomyMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message.ToString());

            await _contentService.UpdateCachedTaxonomyBlobs();

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }
    }
}
