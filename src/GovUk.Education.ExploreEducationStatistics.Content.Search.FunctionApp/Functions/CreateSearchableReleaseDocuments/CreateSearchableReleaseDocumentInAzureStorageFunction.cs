using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments;

public class CreateSearchableReleaseDocumentInAzureStorageFunction(
    EventGridEventHandler eventGridEventHandler,
    ISearchableDocumentCreator searchableDocumentCreator)
{
    [Function("CreateSearchableReleaseDocument")]
    [QueueOutput("%SearchableDocumentCreatedQueueName%")]
    public async Task<SearchDocumentCreatedMessageDto> CreateSearchableReleaseDocumentInAzureStorage(
        [QueueTrigger("%ReleaseVersionPublishedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<ReleaseVersionPublishedEventDto, SearchDocumentCreatedMessageDto>(
            context, 
            eventDto,
            async (payload, ct) =>
            {
                var request = new CreatePublicationLatestReleaseSearchableDocumentRequest{ PublicationSlug = payload.PublicationSlug };
                var response = await searchableDocumentCreator.CreatePublicationLatestReleaseSearchableDocument(request, ct);
                return new SearchDocumentCreatedMessageDto
                {
                    PublicationId = payload.PublicationId,
                    PublicationSlug = response.PublicationSlug,
                    ReleaseId = payload.ReleaseId,
                    ReleaseSlug = payload.ReleaseSlug,
                    ReleaseVersionId = response.ReleaseVersionId,
                    BlobName = response.BlobName,
                    PublicationLatestReleaseVersionId = payload.PublicationLatestReleaseVersionId
                };
            });
}
