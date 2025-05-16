using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived;

public class OnPublicationArchivedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationArchived))]
    [QueueOutput("%RemovePublicationSearchableDocumentsQueueName%")]
    public async Task<RemovePublicationSearchableDocumentsDto[]> OnPublicationArchived(
        [QueueTrigger("%PublicationArchivedQueueName%")] EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<PublicationArchivedEventDto, RemovePublicationSearchableDocumentsDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RemovePublicationSearchableDocumentsDto[]>(
                    string.IsNullOrEmpty(payload.PublicationSlug)
                        ? []
                        : [new RemovePublicationSearchableDocumentsDto { PublicationSlug = payload.PublicationSlug }]));
}
