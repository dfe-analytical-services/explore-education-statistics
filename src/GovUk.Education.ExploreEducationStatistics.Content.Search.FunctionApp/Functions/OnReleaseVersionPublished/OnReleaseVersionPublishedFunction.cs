using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnReleaseVersionPublished.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnReleaseVersionPublished;

public class OnReleaseVersionPublishedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnReleaseVersionPublished))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnReleaseVersionPublished(
        [QueueTrigger("%ReleaseVersionPublishedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<ReleaseVersionPublishedEventDto, RefreshSearchableDocumentMessageDto[]>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult(
                    // TODO: Detect whether this is the latest published release version.
                    // If not, we don't need to refresh the Searchable Document (which is only for the latest release version) so return empty.
                    string.IsNullOrEmpty(payload.PublicationSlug)
                        ? []
                        : new[]
                        {
                            new RefreshSearchableDocumentMessageDto { PublicationSlug = payload.PublicationSlug }
                        }));
}
