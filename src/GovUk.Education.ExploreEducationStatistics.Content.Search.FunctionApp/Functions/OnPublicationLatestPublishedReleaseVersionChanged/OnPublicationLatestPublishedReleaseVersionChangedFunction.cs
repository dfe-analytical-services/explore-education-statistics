using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseVersionChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseVersionChanged;

public class OnPublicationLatestPublishedReleaseVersionChangedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationChanged))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnPublicationLatestPublishedReleaseVersionChanged(
        [QueueTrigger("%PublicationLatestPublishedReleaseVersionChangedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<PublicationLatestPublishedReleaseVersionChangedEventDto, RefreshSearchableDocumentMessageDto[]>(
            context, 
            eventDto,
            (payload, ct) => 
                Task.FromResult<RefreshSearchableDocumentMessageDto[]>(
                    string.IsNullOrEmpty(payload.Slug)
                        ? []
                        : [ new RefreshSearchableDocumentMessageDto { PublicationSlug = payload.Slug } ]));
}
