using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationArchived.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationArchived;

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
                    string.IsNullOrEmpty(payload.Slug)
                        ? []
                        : [new RemovePublicationSearchableDocumentsDto { PublicationSlug = payload.Slug }]));
}
