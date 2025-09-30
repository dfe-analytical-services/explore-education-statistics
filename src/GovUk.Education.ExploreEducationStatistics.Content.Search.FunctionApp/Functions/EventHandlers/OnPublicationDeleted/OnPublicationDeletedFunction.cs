using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted;

public class OnPublicationDeletedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationDeleted))]
    [QueueOutput("%RemoveSearchableDocumentQueueName%")]
    public async Task<RemoveSearchableDocumentDto[]> OnPublicationDeleted(
        [QueueTrigger("%PublicationDeletedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<
            PublicationDeletedEventDto,
            RemoveSearchableDocumentDto[]
        >(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RemoveSearchableDocumentDto[]>(
                    payload.LatestPublishedRelease != null
                        ?
                        [
                            new RemoveSearchableDocumentDto
                            {
                                ReleaseId = payload.LatestPublishedRelease.LatestPublishedReleaseId,
                            },
                        ]
                        : []
                )
        );
}
