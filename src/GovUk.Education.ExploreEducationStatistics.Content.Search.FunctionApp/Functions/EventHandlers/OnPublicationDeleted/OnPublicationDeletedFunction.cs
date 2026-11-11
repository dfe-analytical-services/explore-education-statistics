using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted;

public class OnPublicationDeletedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationDeleted))]
    [QueueOutput("%RemoveReleaseDocumentQueueName%")]
    public async Task<RemoveReleaseDocumentMessageDto[]> OnPublicationDeleted(
        [QueueTrigger("%PublicationDeletedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<PublicationDeletedEventDto, RemoveReleaseDocumentMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult(
                    payload
                        .ReleaseIds?.Distinct()
                        .Select(releaseId => new RemoveReleaseDocumentMessageDto { ReleaseId = releaseId })
                        .ToArray()
                        ?? []
                )
        );
}
