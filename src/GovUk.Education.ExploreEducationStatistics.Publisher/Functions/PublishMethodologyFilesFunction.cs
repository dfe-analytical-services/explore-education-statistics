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
    [Function("PublishMethodologyFiles")]
    public async Task PublishMethodologyFiles(
        [QueueTrigger(PublishMethodologyFilesQueue)] PublishMethodologyFilesMessage message,
        FunctionContext context
    )
    {
        logger.LogInformation(
            "{FunctionName} triggered: {Message}",
            context.FunctionDefinition.Name,
            message
        );

        await publishingService.PublishMethodologyFiles(message.MethodologyId);

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }
}
