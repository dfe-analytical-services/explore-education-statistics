using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument;

public class RefreshSearchableDocumentFunction(
    ISearchableDocumentCreator searchableDocumentCreator,
    ICommandHandler commandHandler)
{
    [Function(nameof(RefreshSearchableDocument))]
    [QueueOutput("%SearchableDocumentCreatedQueueName%")]
    public async Task<SearchableDocumentCreatedMessageDto[]> RefreshSearchableDocument(
        [QueueTrigger("%RefreshSearchableDocumentQueueName%")]
        RefreshSearchableDocumentMessageDto message,
        FunctionContext context) =>
        await commandHandler.Handle(
            RefreshSearchableDocument,
            message,
            context.CancellationToken) ?? [];

    private async Task<SearchableDocumentCreatedMessageDto[]> RefreshSearchableDocument(
        RefreshSearchableDocumentMessageDto message, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.PublicationSlug))
        {
            return [];
        }

        // Create Searchable Document
        var request = new CreatePublicationLatestReleaseSearchableDocumentRequest
        {
            PublicationSlug = message.PublicationSlug
        };

        var response =
            await searchableDocumentCreator.CreatePublicationLatestReleaseSearchableDocument(
                request,
                cancellationToken);

        return
        [
            new SearchableDocumentCreatedMessageDto
            {
                PublicationSlug = response.PublicationSlug,
                ReleaseId = response.ReleaseId,
                ReleaseSlug = response.ReleaseSlug,
                ReleaseVersionId = response.ReleaseVersionId,
                BlobName = response.BlobName,
            }
        ];
    }
}
