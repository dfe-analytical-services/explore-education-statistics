using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationRestored.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationRestored;

public class OnPublicationRestoredFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationRestored))]
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public async Task<RefreshReleaseDocumentMessageDto[]> OnPublicationRestored(
        [QueueTrigger("%PublicationRestoredQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<PublicationRestoredEventDto, RefreshReleaseDocumentMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RefreshReleaseDocumentMessageDto[]>(
                    string.IsNullOrEmpty(payload.PublicationSlug)
                        ? []
                        : [new RefreshReleaseDocumentMessageDto { PublicationSlug = payload.PublicationSlug }]
                )
        );
}
