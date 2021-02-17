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
    public class PublishTaxonomyFunction
    {
        private readonly IContentService _contentService;
        private readonly ITaxonomyService _taxonomyService;

        public PublishTaxonomyFunction(IContentService contentService, ITaxonomyService taxonomyService)
        {
            _contentService = contentService;
            _taxonomyService = taxonomyService;
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

            var context = new PublishContext(DateTime.UtcNow, false);

            await _contentService.UpdateTaxonomy(context);
            await _taxonomyService.SyncTaxonomy();

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }
    }
}
