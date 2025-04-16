using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.
    RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.
    RemovePublicationSearchableDocuments;

public class RemovePublicationSearchableDocumentsFunction(ISearchableDocumentRemover searchableDocumentRemover)
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

        await searchableDocumentRemover.RemovePublicationSearchableDocuments(
            new RemovePublicationSearchableDocumentsRequest { PublicationSlug = message.PublicationSlug },
            context.CancellationToken);
    }
}
