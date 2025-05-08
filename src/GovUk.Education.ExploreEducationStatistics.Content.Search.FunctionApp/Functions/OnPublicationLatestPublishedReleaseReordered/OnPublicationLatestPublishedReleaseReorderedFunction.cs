using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered;

public class OnPublicationLatestPublishedReleaseReorderedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationLatestPublishedReleaseReordered))]
    public async Task<OnPublicationLatestPublishedReleaseReorderedOutput> OnPublicationLatestPublishedReleaseReordered(
        [QueueTrigger("%PublicationLatestPublishedReleaseReorderedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<PublicationLatestPublishedReleaseReorderedEventDto, OnPublicationLatestPublishedReleaseReorderedOutput>(
            context, 
            eventDto,
            (payload, ct) =>
            {
                if (string.IsNullOrEmpty(payload.Slug))
                {
                    return Task.FromResult(OnPublicationLatestPublishedReleaseReorderedOutput.Empty);
                }

                // Create searchable document for the release version that has become the latest version
                RefreshSearchableDocumentMessageDto[] newSearchableDocuments = [new() { PublicationSlug = payload.Slug }];

                // Remove the previous "latest" release version's searchable document 
                RemoveSearchableDocumentDto[] removeSearchableDocuments =
                    payload.PreviousReleaseVersionId.HasNonEmptyValue()
                        ? [ new RemoveSearchableDocumentDto{ ReleaseId = payload.PreviousReleaseVersionId.Value} ]
                        : [];
                
                return Task.FromResult(new OnPublicationLatestPublishedReleaseReorderedOutput
                {
                    RefreshSearchableDocuments = newSearchableDocuments,
                    RemoveSearchableDocuments = removeSearchableDocuments
                });
            });
}
