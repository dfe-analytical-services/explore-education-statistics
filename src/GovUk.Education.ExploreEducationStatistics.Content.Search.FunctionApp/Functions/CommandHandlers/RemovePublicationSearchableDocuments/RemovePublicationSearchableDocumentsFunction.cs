using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.
    RemovePublicationSearchableDocuments;

public class RemovePublicationSearchableDocumentsFunction(
    ILogger<RemovePublicationSearchableDocumentsFunction> logger,
    ISearchableDocumentRemover searchableDocumentRemover)
{
    [Function(nameof(RemovePublicationSearchableDocuments))]
    public async Task RemovePublicationSearchableDocuments(
        [QueueTrigger("%RemovePublicationSearchableDocumentsQueueName%")] RemovePublicationSearchableDocumentsDto message,
        FunctionContext context)
    {
        if (string.IsNullOrEmpty(message.PublicationSlug))
        {
            return;
        }

        var response = await searchableDocumentRemover.RemovePublicationSearchableDocuments(
            new RemovePublicationSearchableDocumentsRequest { PublicationSlug = message.PublicationSlug },
            context.CancellationToken);

        logger.LogInformation(
            "Removed searchable documents for publication \"{PublicationSlug}\". Response: {@response}", message.PublicationSlug, response);
    }
}
