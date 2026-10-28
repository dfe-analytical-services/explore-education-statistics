using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument;

public class RefreshReleaseDocumentFunction(
    ISearchableDocumentCreator searchableDocumentCreator,
    ICommandHandler commandHandler
)
{
    [Function(nameof(RefreshReleaseDocument))]
    [QueueOutput("%ReleaseDocumentCreatedQueueName%")]
    public async Task<ReleaseDocumentCreatedMessageDto[]> RefreshReleaseDocument(
        [QueueTrigger("%RefreshReleaseDocumentQueueName%")] RefreshReleaseDocumentMessageDto message,
        FunctionContext context
    ) => await commandHandler.Handle(RefreshReleaseDocument, message, context.CancellationToken);

    private async Task<ReleaseDocumentCreatedMessageDto[]> RefreshReleaseDocument(
        RefreshReleaseDocumentMessageDto message,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(message.PublicationSlug))
        {
            return [];
        }

        // Create Searchable Document
        var request = new CreatePublicationLatestReleaseSearchableDocumentRequest
        {
            PublicationSlug = message.PublicationSlug,
        };

        var response = await searchableDocumentCreator.CreatePublicationLatestReleaseSearchableDocument(
            request,
            cancellationToken
        );

        return
        [
            new ReleaseDocumentCreatedMessageDto
            {
                PublicationSlug = response.PublicationSlug,
                ReleaseId = response.ReleaseId,
                ReleaseSlug = response.ReleaseSlug,
                ReleaseVersionId = response.ReleaseVersionId,
                BlobName = response.BlobName,
            },
        ];
    }
}
