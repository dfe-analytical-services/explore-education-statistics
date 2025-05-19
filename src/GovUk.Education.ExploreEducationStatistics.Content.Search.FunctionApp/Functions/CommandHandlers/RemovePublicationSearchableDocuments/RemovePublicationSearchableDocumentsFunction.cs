using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.
    RemovePublicationSearchableDocuments;

public class RemovePublicationSearchableDocumentsFunction(
    ILogger<RemovePublicationSearchableDocumentsFunction> logger,
    ISearchableDocumentRemover searchableDocumentRemover,
    ICommandHandler commandHandler)
{
    [Function(nameof(RemovePublicationSearchableDocuments))]
    public async Task RemovePublicationSearchableDocuments(
        [QueueTrigger("%RemovePublicationSearchableDocumentsQueueName%")] RemovePublicationSearchableDocumentsDto message,
        FunctionContext context) =>
        await commandHandler.Handle(RemovePublicationSearchableDocuments, message, context.CancellationToken);
    
    private async Task RemovePublicationSearchableDocuments(
        RemovePublicationSearchableDocumentsDto message, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.PublicationSlug))
        {
            return;
        }

        var response = await searchableDocumentRemover.RemovePublicationSearchableDocuments(
            new RemovePublicationSearchableDocumentsRequest { PublicationSlug = message.PublicationSlug },
            cancellationToken);

        logger.LogInformation(
            "Removed searchable documents for publication \"{PublicationSlug}\". Response: {@response}", 
            message.PublicationSlug, 
            response);
    }
}
