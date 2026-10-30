using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished;

public class OnReleaseVersionPublishedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnReleaseVersionPublished))]
    public async Task<OnReleaseVersionPublishedOutput> OnReleaseVersionPublished(
        [QueueTrigger("%ReleaseVersionPublishedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
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
                            RefreshReleaseDocumentMessages = BuildRefreshReleaseDocumentMessages(payload),
                            RemoveReleaseDocuments = BuildRemoveReleaseDocumentsCommands(payload),
                        }
                )
        );

    private static RefreshReleaseDocumentMessageDto[] BuildRefreshReleaseDocumentMessages(
        ReleaseVersionPublishedEventDto payload
    ) => [new() { PublicationSlug = payload.PublicationSlug }];

    private static RemoveReleaseDocumentDto[] BuildRemoveReleaseDocumentsCommands(
        ReleaseVersionPublishedEventDto payload
    ) =>
        payload
            is { NewlyPublishedReleaseVersionIsForDifferentRelease: true, PreviousLatestPublishedReleaseId: not null }
            ? [new RemoveReleaseDocumentDto { ReleaseId = payload.PreviousLatestPublishedReleaseId }]
            : [];
}

public record OnReleaseVersionPublishedOutput
{
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public RefreshReleaseDocumentMessageDto[] RefreshReleaseDocumentMessages { get; init; } = [];

    [QueueOutput("%RemoveReleaseDocumentQueueName%")]
    public RemoveReleaseDocumentDto[] RemoveReleaseDocuments { get; init; } = [];

    public static OnReleaseVersionPublishedOutput Empty => new();
}
