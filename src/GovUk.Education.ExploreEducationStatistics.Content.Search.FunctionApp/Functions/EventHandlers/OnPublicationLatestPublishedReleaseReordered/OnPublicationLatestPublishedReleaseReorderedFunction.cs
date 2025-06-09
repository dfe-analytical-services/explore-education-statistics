using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered;

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
            (payload, _) =>
            {
                if (string.IsNullOrEmpty(payload.Slug))
                {
                    return Task.FromResult(OnPublicationLatestPublishedReleaseReorderedOutput.Empty);
                }

                // Create searchable document for the release version that has become the latest version
                RefreshSearchableDocumentMessageDto[] newSearchableDocuments = [new() { PublicationSlug = payload.Slug }];

                // Remove the previous "latest" release version's searchable document 
                RemoveSearchableDocumentDto[] removeSearchableDocuments =
                    payload.PreviousReleaseId.HasNonEmptyValue()
                        ? [ new RemoveSearchableDocumentDto{ ReleaseId = payload.PreviousReleaseId.Value} ]
                        : [];
                
                return Task.FromResult(new OnPublicationLatestPublishedReleaseReorderedOutput
                {
                    RefreshSearchableDocuments = newSearchableDocuments,
                    RemoveSearchableDocuments = removeSearchableDocuments
                });
            });
}
