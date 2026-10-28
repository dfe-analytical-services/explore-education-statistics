using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationReleaseDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived;

public class OnPublicationArchivedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationArchived))]
    [QueueOutput("%RemovePublicationReleaseDocumentsQueueName%")]
    public async Task<RemovePublicationReleaseDocumentsDto[]> OnPublicationArchived(
        [QueueTrigger("%PublicationArchivedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<PublicationArchivedEventDto, RemovePublicationReleaseDocumentsDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RemovePublicationReleaseDocumentsDto[]>(
                    string.IsNullOrEmpty(payload.PublicationSlug)
                        ? []
                        : [new RemovePublicationReleaseDocumentsDto { PublicationSlug = payload.PublicationSlug }]
                )
        );
}
