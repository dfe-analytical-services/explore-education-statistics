using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseSlugChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseSlugChanged;

public class OnReleaseSlugChangedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnReleaseSlugChangedEvent))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnReleaseSlugChangedEvent(
        [QueueTrigger("%ReleaseSlugChangedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<ReleaseSlugChangedEventDto, RefreshSearchableDocumentMessageDto[]>(
            context, 
            eventDto,
            (payload, _) =>
                Task.FromResult(
                    string.IsNullOrEmpty(payload.PublicationSlug)
                    ? []
                    : new []
                    {
                        new RefreshSearchableDocumentMessageDto
                        {
                            PublicationSlug = payload.PublicationSlug
                        }
                    }));
}
