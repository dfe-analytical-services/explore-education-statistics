using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument;

public class RefreshSearchableDocumentFunction(ISearchableDocumentCreator searchableDocumentCreator)
{
    [Function("RefreshSearchableDocument")]
    [QueueOutput("%SearchableDocumentCreatedQueueName%")]
    public async Task<SearchableDocumentCreatedMessageDto> OnReleaseVersionPublished(
        [QueueTrigger("%RefreshSearchableDocumentQueueName%")]
        RefreshSearchableDocumentMessageDto message,
        FunctionContext context)
    {
        // Create Searchable Document
        var request = new CreatePublicationLatestReleaseSearchableDocumentRequest
            {
                PublicationSlug = message.PublicationSlug
            };
        
        var response = await searchableDocumentCreator.CreatePublicationLatestReleaseSearchableDocument(request, context.CancellationToken);
        
        return new SearchableDocumentCreatedMessageDto
        {
            PublicationSlug = response.PublicationSlug,
            ReleaseId = response.ReleaseId,
            ReleaseSlug = response.ReleaseSlug,
            ReleaseVersionId = response.ReleaseVersionId,
            BlobName = response.BlobName,
        };
    }
}
