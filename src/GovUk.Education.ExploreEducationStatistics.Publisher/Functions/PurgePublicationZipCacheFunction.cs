using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PurgePublicationZipCacheFunction(
    IReleaseVersionRepository releaseVersionRepository,
    IFrontDoorCacheService frontDoorCacheService
)
{
    [Function(nameof(PurgePublicationZipCache))]
    public async Task PurgePublicationZipCache(
        [QueueTrigger(PublicationZipPurgeQueue)] EventGridEvent publicationChangedEvent,
        CancellationToken cancellationToken
    )
    {
        if (!Guid.TryParse(publicationChangedEvent.Subject, out var publicationId))
        {
            throw new ArgumentException("PublicationChanged event subject must be a publication ID.");
        }

        var releaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(
            publicationId,
            publishedOnly: true,
            cancellationToken: cancellationToken
        );

        await frontDoorCacheService.PurgeAllFilesZipCache(releaseVersionIds.ToHashSet(), cancellationToken);
    }
}
