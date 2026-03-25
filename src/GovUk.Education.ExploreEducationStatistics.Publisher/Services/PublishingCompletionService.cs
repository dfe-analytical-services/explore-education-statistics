using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class PublishingCompletionService(
    ContentDbContext contentDbContext,
    IContentService contentService,
    IMethodologyService methodologyService,
    INotificationsService notificationsService,
    IReleasePublishingStatusService releasePublishingStatusService,
    IReleaseService releaseService,
    IRedirectsCacheService redirectsCacheService,
    IDataSetPublishingService dataSetPublishingService,
    IPublisherEventRaiser publisherEventRaiser,
    IEducationInNumbersService educationInNumbersService
) : IPublishingCompletionService
{
    public async Task CompletePublishingIfAllPriorStagesComplete(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys,
        CancellationToken cancellationToken = default
    )
    {
        if (!releasePublishingKeys.Any())
        {
            return;
        }

        var releaseStatuses = await releasePublishingKeys
            .ToAsyncEnumerable()
            .Select(
                async (ReleasePublishingKey key, CancellationToken _) => await releasePublishingStatusService.Get(key)
            )
            .ToListAsync(cancellationToken);

        var prePublishingStagesComplete = releaseStatuses
            .Where(status => status.AllStagesPriorToPublishingComplete())
            .ToList();

        if (!prePublishingStagesComplete.Any())
        {
            return;
        }

        foreach (var status in prePublishingStagesComplete)
        {
            await releasePublishingStatusService.UpdatePublishingStage(
                status.AsTableRowKey(),
                ReleasePublishingStatusPublishingStage.Started
            );
        }

        var releaseVersionIdsToUpdate = prePublishingStagesComplete.Select(status => status.ReleaseVersionId).ToArray();
        foreach (var releaseVersionId in releaseVersionIdsToUpdate)
        {
            await releaseService.CompletePublishing(releaseVersionId, DateTimeOffset.UtcNow);
        }

        foreach (var releaseVersionId in releaseVersionIdsToUpdate)
        {
            var releaseVersion = await releaseService.Get(releaseVersionId);
            var methodologyVersions = await methodologyService.GetLatestVersionByRelease(releaseVersion);

            foreach (var methodologyVersion in methodologyVersions)
            {
                if (await methodologyService.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
                {
                    await methodologyService.Publish(methodologyVersion);
                }
            }
        }

        var publishedReleaseVersionInfos = await BuildPublishedReleaseVersionInfos(releaseVersionIdsToUpdate);
        var publishedPublicationInfos = await PublishPublications(publishedReleaseVersionInfos, cancellationToken);

        await HandleArchivedPublications(publishedPublicationInfos, cancellationToken);

        await contentService.DeletePreviousVersionsDownloadFiles(releaseVersionIdsToUpdate);
        await contentService.DeletePreviousVersionsContent(releaseVersionIdsToUpdate);

        await notificationsService.NotifySubscribersIfApplicable(releaseVersionIdsToUpdate);

        await notificationsService.SendReleasePublishingFeedbackEmails(releaseVersionIdsToUpdate);

        // Update the cached trees in case any methodologies/publications
        // are now accessible for the first time after publishing these releases
        await contentService.UpdateCachedTaxonomyBlobs();

        await redirectsCacheService.UpdateRedirects();

        await dataSetPublishingService.PublishDataSets(releaseVersionIdsToUpdate);

        await educationInNumbersService.UpdateEinTiles(releaseVersionIdsToUpdate, cancellationToken);

        await publisherEventRaiser.OnReleaseVersionsPublished(publishedPublicationInfos);

        foreach (var status in prePublishingStagesComplete)
        {
            await releasePublishingStatusService.UpdatePublishingStage(
                status.AsTableRowKey(),
                ReleasePublishingStatusPublishingStage.Complete
            );
        }
    }

    private async ValueTask<List<PublishedReleaseVersionInfo>> BuildPublishedReleaseVersionInfos(
        IReadOnlyList<Guid> releaseVersionIds
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => releaseVersionIds.Contains(rv.Id))
            .Select(rv => new PublishedReleaseVersionInfo
            {
                ReleaseVersionId = rv.Id,
                ReleaseId = rv.ReleaseId,
                ReleaseSlug = rv.Release.Slug,
                PublicationId = rv.Release.PublicationId,
            })
            .ToListAsync();
    }

    private async ValueTask<List<PublishedPublicationInfo>> PublishPublications(
        IReadOnlyList<PublishedReleaseVersionInfo> releaseVersions,
        CancellationToken cancellationToken
    )
    {
        return await releaseVersions
            .GroupBy(info => info.PublicationId)
            .ToAsyncEnumerable()
            .Select(
                async (group, ct) =>
                    await PublishPublication(
                        publicationId: group.Key,
                        publishedReleaseVersions: group.ToList(),
                        cancellationToken: ct
                    )
            )
            .ToListAsync(cancellationToken);
    }

    private async ValueTask<PublishedPublicationInfo> PublishPublication(
        Guid publicationId,
        IReadOnlyList<PublishedReleaseVersionInfo> publishedReleaseVersions,
        CancellationToken cancellationToken
    )
    {
        var publication = await contentDbContext
            .Publications.Include(p => p.LatestPublishedReleaseVersion)
            .Include(p => p.SupersededBy)
            .SingleAsync(p => p.Id == publicationId, cancellationToken: cancellationToken);

        var previousLatestPublishedReleaseVersion = publication.LatestPublishedReleaseVersion;

        // Update the latest published releaseVersion for the publication
        var latestPublishedReleaseVersion = await releaseService.GetLatestPublishedReleaseVersion(publicationId);
        publication.LatestPublishedReleaseVersionId = latestPublishedReleaseVersion.Id;
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return new PublishedPublicationInfo
        {
            PublicationId = publication.Id,
            PublicationSlug = publication.Slug,
            LatestPublishedReleaseId = latestPublishedReleaseVersion.ReleaseId,
            LatestPublishedReleaseVersionId = latestPublishedReleaseVersion.Id,
            PreviousLatestPublishedReleaseId = previousLatestPublishedReleaseVersion?.ReleaseId,
            PreviousLatestPublishedReleaseVersionId = previousLatestPublishedReleaseVersion?.Id,
            PublishedReleaseVersions = publishedReleaseVersions,
            IsPublicationArchived = publication.IsArchived(),
        };
    }

    private async Task HandleArchivedPublications(
        IReadOnlyList<PublishedPublicationInfo> publications,
        CancellationToken cancellationToken
    )
    {
        // Archived publications are those replaced by another publication with at least one published release.
        // Filter publications from this run, excluding those already published.
        var newlyPublishedPublicationIds = publications
            .Where(p => !p.WasAlreadyPublished)
            .Select(p => p.PublicationId)
            .ToList();

        // Find publications that should now be archived because they have been replaced by these publications
        var archivedPublications = await contentDbContext
            .Publications.Where(p =>
                p.SupersededById != null && newlyPublishedPublicationIds.Contains(p.SupersededById.Value)
            )
            .ToListAsync(cancellationToken);

        foreach (var archivedPublication in archivedPublications)
        {
            // Raise an event to indicate the publication has been archived
            await publisherEventRaiser.OnPublicationArchived(
                archivedPublication.Id,
                archivedPublication.Slug,
                archivedPublication.SupersededById!.Value
            );
        }
    }
}
