using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

public class PublishTaxonomyFunction(ILogger<PublishTaxonomyFunction> logger, IContentService contentService)
{
    /// <summary>
    /// Azure function that updates the cached methodologies and publications 'tree' models
    /// used by the public site Methodologies, Data Catalogue, and Table Tool pages.
    /// </summary>
    /// <remarks>
    /// Triggered via a message queued by the Admin application when a theme is created/updated/deleted.
    /// </remarks>
    [Function(nameof(PublishTaxonomy))]
    public async Task PublishTaxonomy(
        [QueueTrigger(PublishTaxonomyQueue)] PublishTaxonomyMessage ignored, // The binding name _ is invalid
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        await contentService.UpdateCachedTaxonomyBlobs();

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }
}
