using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationChanged;

public class OnPublicationChangedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationChanged))]
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public async Task<RefreshReleaseDocumentMessageDto[]> OnPublicationChanged(
        [QueueTrigger("%PublicationChangedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<PublicationChangedEventDto, RefreshReleaseDocumentMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RefreshReleaseDocumentMessageDto[]>(
                    string.IsNullOrEmpty(payload.Slug) || payload.IsPublicationArchived == true
                        ? []
                        : [new RefreshReleaseDocumentMessageDto { PublicationSlug = payload.Slug }]
                )
        );
}
