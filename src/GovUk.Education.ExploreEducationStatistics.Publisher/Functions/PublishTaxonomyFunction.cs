using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PublishTaxonomyFunction(
    ILogger<PublishTaxonomyFunction> logger,
    IContentService contentService)
{
    [Function("PublishTaxonomy")]
    public async Task PublishTaxonomy(
#pragma warning disable IDE0060
        [QueueTrigger(PublishTaxonomyQueue)] PublishTaxonomyMessage message,
#pragma warning restore IDE0060
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        await contentService.UpdateCachedTaxonomyBlobs();

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }
}
