using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted;

public class OnPublicationDeletedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationDeleted))]
    [QueueOutput("%RemoveReleaseDocumentQueueName%")]
    public async Task<RemoveReleaseDocumentDto[]> OnPublicationDeleted(
        [QueueTrigger("%PublicationDeletedQueueName%")] EventGridEvent eventDto,
        FunctionContext context
    ) =>
        await eventGridEventHandler.Handle<PublicationDeletedEventDto, RemoveReleaseDocumentDto[]>(
            context,
            eventDto,
            (payload, _) => Task.FromResult(BuildRemoveReleaseDocumentCommands(payload))
        );

    /// <summary>
    /// Builds a removal command for every Release of the deleted Publication that could have a release
    /// document.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Publication is normally represented by a single release document, named after its latest
    /// published Release, and the document for the Release it supersedes is removed as that changes. A stale
    /// document is left behind whenever the removal half of that exchange does not happen, which is why
    /// archiving a Publication sweeps all of its Releases rather than only the latest one - see
    /// RemovePublicationReleaseDocumentsFunction.
    /// </para>
    /// <para>
    /// Deletion cannot sweep the same way, as that command resolves a Publication's Release ids through the
    /// Content API and the Publication no longer exists by the time this runs. The Release ids therefore
    /// travel on the event itself. Removing a document that was never there is a no-op.
    /// </para>
    /// </remarks>
    private static RemoveReleaseDocumentDto[] BuildRemoveReleaseDocumentCommands(PublicationDeletedEventDto payload)
    {
        var releaseIds = new List<Guid?>(payload.ReleaseIds?.Cast<Guid?>() ?? []);

        // Events raised before ReleaseIds was added to the payload carry only the latest published Release.
        releaseIds.Add(payload.LatestPublishedRelease?.LatestPublishedReleaseId);

        return
        [
            .. releaseIds
                .Where(releaseId => !releaseId.IsBlank())
                .Distinct()
                .Select(releaseId => new RemoveReleaseDocumentDto { ReleaseId = releaseId }),
        ];
    }
}
