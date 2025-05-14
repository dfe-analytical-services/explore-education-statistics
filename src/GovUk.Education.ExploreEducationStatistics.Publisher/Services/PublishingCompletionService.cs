using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
    IPublicationCacheService publicationCacheService,
    IReleaseService releaseService,
    IRedirectsCacheService redirectsCacheService,
    IDataSetPublishingService dataSetPublishingService,
    IPublisherEventRaiser publisherEventRaiser)
    : IPublishingCompletionService
{
    public async Task CompletePublishingIfAllPriorStagesComplete(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        var releaseStatuses = await releasePublishingKeys
            .ToAsyncEnumerable()
            .SelectAwait(async key => await releasePublishingStatusService.Get(key))
            .ToListAsync();

        var prePublishingStagesComplete = releaseStatuses
            .Where(status => status.AllStagesPriorToPublishingComplete())
            .ToList();

        if (!prePublishingStagesComplete.Any())
        {
            return;
        }

        await prePublishingStagesComplete
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(status => releasePublishingStatusService.UpdatePublishingStage(status.AsTableRowKey(),
                ReleasePublishingStatusPublishingStage.Started));

        var releaseVersionIdsToUpdate = prePublishingStagesComplete
            .Select(status => status.ReleaseVersionId)
            .ToArray();

        await releaseVersionIdsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(releaseId => releaseService.CompletePublishing(releaseId, DateTime.UtcNow));

        var publishedReleaseVersionInfos = await BuildPublishedReleaseVersionInfos(releaseVersionIdsToUpdate);

        await releaseVersionIdsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async releaseVersionId =>
            {
                var releaseVersion = await releaseService.Get(releaseVersionId);
                var methodologyVersions =
                    await methodologyService.GetLatestVersionByRelease(releaseVersion);

                if (!methodologyVersions.Any())
                {
                    return;
                }

                foreach (var methodologyVersion in methodologyVersions)
                {
                    if (await methodologyService.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
                    {
                        await methodologyService.Publish(methodologyVersion);
                    }
                }
            });

        var publishedPublicationInfos = await PublishPublications(publishedReleaseVersionInfos);

        // Archive any publications that are superseded by newly published ones.
        await HandleArchivedPublications(publishedPublicationInfos);

        await contentService.DeletePreviousVersionsDownloadFiles(releaseVersionIdsToUpdate);
        await contentService.DeletePreviousVersionsContent(releaseVersionIdsToUpdate);

        await notificationsService.NotifySubscribersIfApplicable(releaseVersionIdsToUpdate);

        // Update the cached trees in case any methodologies/publications
        // are now accessible for the first time after publishing these releases
        await contentService.UpdateCachedTaxonomyBlobs();

        await redirectsCacheService.UpdateRedirects();

        await dataSetPublishingService.PublishDataSets(releaseVersionIdsToUpdate);

        // TODO EES-6107 this info doesn't have all the fields in it yet!
        await publisherEventRaiser.RaiseReleaseVersionPublishedEvents(publishedReleaseVersionInfos);

        await prePublishingStagesComplete
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async status =>
                await releasePublishingStatusService
                    .UpdatePublishingStage(status.AsTableRowKey(), ReleasePublishingStatusPublishingStage.Complete));
    }

    private async ValueTask<List<PublishedReleaseVersionInfo>>
        BuildPublishedReleaseVersionInfos(IReadOnlyList<Guid> releaseVersionIds)
    {
        return await contentDbContext
            .ReleaseVersions
            .Where(rv => releaseVersionIds.Contains(rv.Id))
            .Select(rv => new PublishedReleaseVersionInfo
            {
                ReleaseVersionId = rv.Id,
                ReleaseId = rv.ReleaseId,
                ReleaseSlug = rv.Release.Slug,
                PublicationId = rv.Release.PublicationId,
                PublicationSlug = rv.Release.Publication.Slug
            })
            .ToListAsync();
    }

    private async ValueTask<List<PublishedPublicationInfo>> PublishPublications(
        IReadOnlyList<PublishedReleaseVersionInfo> releaseVersions)
    {
        var publicationIds = releaseVersions
            .Select(info => info.PublicationId)
            .Distinct()
            .ToList();

        return await publicationIds
            .ToAsyncEnumerable()
            .SelectAwait(PublishPublication)
            .ToListAsync();
    }

    private async ValueTask<PublishedPublicationInfo> PublishPublication(Guid publicationId)
    {
        var publication = await contentDbContext.Publications
            .SingleAsync(p => p.Id == publicationId);

        var initialLatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId;

        // Update the latest published releaseVersion for the publication
        var latestPublishedReleaseVersion = await releaseService.GetLatestPublishedReleaseVersion(publicationId);
        publication.LatestPublishedReleaseVersionId = latestPublishedReleaseVersion.Id;
        await contentDbContext.SaveChangesAsync();

        // Invalidate the cache for the publication
        await publicationCacheService.UpdatePublication(publication.Slug);

        return new PublishedPublicationInfo
        {
            PublicationId = publication.Id,
            PublicationSlug = publication.Slug,
            InitialLatestPublishedReleaseVersionId = initialLatestPublishedReleaseVersionId,
            LatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId.Value
        };
    }

    private async Task HandleArchivedPublications(IReadOnlyList<PublishedPublicationInfo> publications)
    {
        // Archived publications are those superseded by another with at least one published release.
        // Select publications affected by this publishing run, excluding those that already had a published release.
        var publicationIds = publications
            .Where(p => !p.WasAlreadyPublished)
            .Select(p => p.PublicationId)
            .ToList();

        // Find all publications that are now archived by these publications
        var archivedPublications = await contentDbContext.Publications
            .Where(p => p.SupersededById != null && publicationIds.Contains(p.SupersededById.Value))
            .ToListAsync();

        foreach (var archivedPublication in archivedPublications)
        {
            // Clear the cache of the publication to reflect its new state
            await publicationCacheService.UpdatePublication(archivedPublication.Slug);
        }
    }
}
