using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationRestored.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationRestored;

public class OnPublicationRestoredFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationRestored))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnPublicationRestored(
        [QueueTrigger("%PublicationRestoredQueueName%")] EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<PublicationRestoredEventDto, RefreshSearchableDocumentMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult<RefreshSearchableDocumentMessageDto[]>(
                    string.IsNullOrEmpty(payload.PublicationSlug)
                        ? []
                        : [new RefreshSearchableDocumentMessageDto { PublicationSlug = payload.PublicationSlug }]));
}
