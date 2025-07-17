using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;
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
                    string.IsNullOrEmpty(payload.PublicationSlug) 
                    || !payload.NewlyPublishedReleaseVersionIsLatest 
                    || payload.IsPublicationArchived == true
                        ? OnReleaseVersionPublishedOutput.Empty
                        : new OnReleaseVersionPublishedOutput
                        {
                            RefreshSearchableDocumentMessages = BuildRefreshSearchableDocumentMessages(payload),
                            RemoveSearchableDocuments = BuildRemoveSearchableDocumentsCommands(payload)
                        }));

    private static RefreshSearchableDocumentMessageDto[] BuildRefreshSearchableDocumentMessages(ReleaseVersionPublishedEventDto payload) => 
        [ new() { PublicationSlug = payload.PublicationSlug } ];

    private static RemoveSearchableDocumentDto[] BuildRemoveSearchableDocumentsCommands(ReleaseVersionPublishedEventDto payload) =>
        payload is
        {
            NewlyPublishedReleaseVersionIsForDifferentRelease: true, 
            PreviousLatestPublishedReleaseId: not null
        }
            ? [ new RemoveSearchableDocumentDto { ReleaseId = payload.PreviousLatestPublishedReleaseId } ]
            : [];
}

public record OnReleaseVersionPublishedOutput
{
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public RefreshSearchableDocumentMessageDto[] RefreshSearchableDocumentMessages { get; init; } = [];
    
    [QueueOutput("%RemoveSearchableDocumentQueueName%")]
    public RemoveSearchableDocumentDto[] RemoveSearchableDocuments { get; init; } = [];
    
    public static OnReleaseVersionPublishedOutput Empty => new();
}
