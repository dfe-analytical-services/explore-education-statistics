using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationChanged;

public class OnPublicationChangedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationChanged))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnPublicationChanged(
        [QueueTrigger("%PublicationChangedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<PublicationChangedEventDto, RefreshSearchableDocumentMessageDto[]>(
            context, 
            eventDto,
            (payload, ct) => 
                Task.FromResult<RefreshSearchableDocumentMessageDto[]>(
                    string.IsNullOrEmpty(payload.Slug)
                    ? []
                    : [ new RefreshSearchableDocumentMessageDto { PublicationSlug = payload.Slug } ]));
}
