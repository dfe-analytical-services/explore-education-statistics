using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationReleaseDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationReleaseDocuments;

public class RemovePublicationReleaseDocumentsFunction(
    ILogger<RemovePublicationReleaseDocumentsFunction> logger,
    ISearchableDocumentRemover searchableDocumentRemover,
    ICommandHandler commandHandler
)
{
    [Function(nameof(RemovePublicationReleaseDocuments))]
    public async Task RemovePublicationReleaseDocuments(
        [QueueTrigger("%RemovePublicationReleaseDocumentsQueueName%")] RemovePublicationReleaseDocumentsDto message,
        FunctionContext context
    ) => await commandHandler.Handle(RemovePublicationReleaseDocuments, message, context.CancellationToken);

    private async Task RemovePublicationReleaseDocuments(
        RemovePublicationReleaseDocumentsDto message,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(message.PublicationSlug))
        {
            return;
        }

        var response = await searchableDocumentRemover.RemovePublicationSearchableDocuments(
            new RemovePublicationSearchableDocumentsRequest { PublicationSlug = message.PublicationSlug },
            cancellationToken
        );

        logger.LogInformation(
            """Removed searchable documents for publication "{PublicationSlug}". Response: {@response}""",
            message.PublicationSlug,
            response
        );
    }
}
