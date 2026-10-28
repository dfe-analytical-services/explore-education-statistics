using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseSlugChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseSlugChanged;

public class OnReleaseSlugChangedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnReleaseSlugChangedEvent))]
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public async Task<RefreshReleaseDocumentMessageDto[]> OnReleaseSlugChangedEvent(
        [QueueTrigger("%ReleaseSlugChangedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<ReleaseSlugChangedEventDto, RefreshReleaseDocumentMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult(
                    string.IsNullOrEmpty(payload.PublicationSlug) || payload.IsPublicationArchived == true
                        ? []
                        : new[] { new RefreshReleaseDocumentMessageDto { PublicationSlug = payload.PublicationSlug } }
                )
        );
}
