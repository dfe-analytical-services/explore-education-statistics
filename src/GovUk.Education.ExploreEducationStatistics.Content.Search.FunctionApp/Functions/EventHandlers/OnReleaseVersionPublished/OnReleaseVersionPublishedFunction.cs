using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished;

public class OnReleaseVersionPublishedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnReleaseVersionPublished))]
    public async Task<OnReleaseVersionPublishedOutput> OnReleaseVersionPublished(
        [QueueTrigger("%ReleaseVersionPublishedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<ReleaseVersionPublishedEventDto, OnReleaseVersionPublishedOutput>(
            context,
            eventDto,
            (payload, _) =>
                Task.FromResult(
                    string.IsNullOrEmpty(payload.PublicationSlug) || !payload.NewlyPublishedReleaseVersionIsLatest
                        ? OnReleaseVersionPublishedOutput.Empty
                        : new OnReleaseVersionPublishedOutput
                        {
                            RefreshSearchableDocumentMessages = 
                                [ new RefreshSearchableDocumentMessageDto { PublicationSlug = payload.PublicationSlug } ],
                            RemoveSearchableDocuments = payload.NewlyPublishedReleaseVersionIsForDifferentRelease
                                ? [ new RemoveSearchableDocumentDto { ReleaseId = payload.PreviousLatestReleaseId } ]
                                : []
                        }));
}

public record OnReleaseVersionPublishedOutput
{
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public RefreshSearchableDocumentMessageDto[] RefreshSearchableDocumentMessages { get; init; } = [];
    
    [QueueOutput("%RemoveSearchableDocumentQueueName%")]
    public RemoveSearchableDocumentDto[] RemoveSearchableDocuments { get; init; } = [];
    
    public static OnReleaseVersionPublishedOutput Empty => new();
}
