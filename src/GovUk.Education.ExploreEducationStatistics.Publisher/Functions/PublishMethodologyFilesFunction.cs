using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PublishMethodologyFilesFunction(
    ILogger<PublishMethodologyFilesFunction> logger,
    IPublishingService publishingService
)
{
    /// <summary>
    /// Azure function that copies methodology version files from private to public storage accounts.
    /// </summary>
    /// <remarks>
    /// Triggered via a message queued by the Admin application when a methodology version is approved for immediate
    /// public access, independently of a release version being published.
    /// This differs from the standard immediate and scheduled publication flows when a methodology version is published
    /// alongside a release version, where methodology version files are copied as part of the release version's publishing process.
    /// </remarks>
    [Function(nameof(PublishMethodologyFiles))]
    public async Task PublishMethodologyFiles(
        [QueueTrigger(PublishMethodologyFilesQueue)] PublishMethodologyFilesMessage message,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered: {Message}", context.FunctionDefinition.Name, message);

        await publishingService.PublishMethodologyFiles(message.MethodologyId);

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }
}
