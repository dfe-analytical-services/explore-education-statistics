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
    public async Task<RemovePublicationReleaseDocumentsMessageDto[]> OnPublicationArchived(
        [QueueTrigger("%PublicationArchivedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<PublicationArchivedEventDto, RemovePublicationReleaseDocumentsMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RemovePublicationReleaseDocumentsMessageDto[]>(
                    string.IsNullOrEmpty(payload.PublicationSlug)
                        ? []
                        :
                        [
                            new RemovePublicationReleaseDocumentsMessageDto
                            {
                                PublicationSlug = payload.PublicationSlug,
                            },
                        ]
                )
        );
}
