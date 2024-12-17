using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
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
    IDataSetPublishingService dataSetPublishingService)
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

        var directlyRelatedPublicationIds = await contentDbContext
            .ReleaseVersions
            .Where(rv => releaseVersionIdsToUpdate.Contains(rv.Id))
            .Select(rv => rv.PublicationId)
            .Distinct()
            .ToListAsync();

        await directlyRelatedPublicationIds
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(UpdateLatestPublishedReleaseVersionForPublication);

        // Update the cached publication and any cached superseded publications.
        // If this is the first live release of the publication, the superseding is now enforced
        var publicationSlugsToUpdate = await contentDbContext
            .Publications
            .Where(p => directlyRelatedPublicationIds.Contains(p.Id) ||
                        (p.SupersededById != null && directlyRelatedPublicationIds.Contains(p.SupersededById.Value)))
            .Select(p => p.Slug)
            .ToListAsync();

        await publicationSlugsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(publicationCacheService.UpdatePublication);

        await contentService.DeletePreviousVersionsDownloadFiles(releaseVersionIdsToUpdate);
        await contentService.DeletePreviousVersionsContent(releaseVersionIdsToUpdate);

        await notificationsService.NotifySubscribersIfApplicable(releaseVersionIdsToUpdate);

        // Update the cached trees in case any methodologies/publications
        // are now accessible for the first time after publishing these releases
        await contentService.UpdateCachedTaxonomyBlobs();

        await redirectsCacheService.UpdateRedirects();

        await dataSetPublishingService.PublishDataSets(releaseVersionIdsToUpdate);

        await prePublishingStagesComplete
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async status =>
                await releasePublishingStatusService
                    .UpdatePublishingStage(status.AsTableRowKey(), ReleasePublishingStatusPublishingStage.Complete));
    }

    private async Task UpdateLatestPublishedReleaseVersionForPublication(Guid publicationId)
    {
        var publication = await contentDbContext.Publications
            .SingleAsync(p => p.Id == publicationId);

        // Get the publications release id's by the order they appear in the release series
        var releaseSeriesReleaseIds = publication.ReleaseSeries.ReleaseIds();

        // Work out the publication's new latest published release version.
        // This is the latest published version of the first release which has a published version
        Guid? latestPublishedReleaseVersionId = null;
        foreach (var releaseId in releaseSeriesReleaseIds)
        {
            latestPublishedReleaseVersionId = (await contentDbContext.ReleaseVersions
                .LatestReleaseVersion(releaseId: releaseId, publishedOnly: true)
                .SingleOrDefaultAsync())?.Id;

            if (latestPublishedReleaseVersionId != null)
            {
                break;
            }
        }

        publication.LatestPublishedReleaseVersionId =
            latestPublishedReleaseVersionId ??
            throw new InvalidOperationException(
                $"No latest published release version found for publication {publicationId}");

        contentDbContext.Update(publication);
        await contentDbContext.SaveChangesAsync();
    }
}
