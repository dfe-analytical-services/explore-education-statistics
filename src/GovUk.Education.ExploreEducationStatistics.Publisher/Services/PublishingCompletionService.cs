using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    IEducationInNumbersService educationInNumbersService,
    ILogger<PublishingCompletionService> logger
) : IPublishingCompletionService
{
    /// <summary>
    /// Sets release versions as publicly accessible and executes all tasks required to complete the publishing process,
    /// including publishing any methodology versions being published alongside the release versions.
    /// </summary>
    /// <param name="releasePublishingKeys">A list containing the release version and release status IDs for the release versions to complete publishing for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    public async Task CompletePublishing(
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

        foreach (var status in releaseStatuses)
        {
            await releasePublishingStatusService.UpdatePublishingStage(
                status.AsTableRowKey(),
                ReleasePublishingStatusPublishingStage.Started
            );
        }

        var releaseVersionIdsToUpdate = releaseStatuses.Select(status => status.ReleaseVersionId).ToArray();
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

        await contentService.DeletePreviousVersionsContent(releaseVersionIdsToUpdate);

        // Update the cached trees in case any methodologies/publications
        // are now accessible for the first time after publishing these releases
        await contentService.UpdateCachedTaxonomyBlobs();

        await redirectsCacheService.UpdateRedirects();

        await dataSetPublishingService.PublishDataSets(releaseVersionIdsToUpdate);

        await publisherEventRaiser.OnReleaseVersionsPublished(publishedPublicationInfos);

        await RunPostPublishingTasks(releaseVersionIdsToUpdate, cancellationToken);

        foreach (var status in releaseStatuses)
        {
            await releasePublishingStatusService.UpdatePublishingStage(
                status.AsTableRowKey(),
                ReleasePublishingStatusPublishingStage.Complete
            );
        }
    }

    /// <summary>
    /// Runs tasks that should happen after publishing but must not prevent the release versions from going live.
    /// Failures are logged rather than rethrown, and each task runs regardless of whether a previous one failed.
    /// </summary>
    // TODO EES-7752 Move these tasks somewhere they can run in parallel, separately from publishing, so they can't
    // potentially block the current publication from being published.
    private async Task RunPostPublishingTasks(Guid[] releaseVersionIds, CancellationToken cancellationToken)
    {
        await RunPostPublishingTask(
            nameof(IContentService.InvalidatePreviousVersionsDownloadFiles),
            releaseVersionIds,
            cancellationToken,
            () => contentService.InvalidatePreviousVersionsDownloadFiles(releaseVersionIds)
        );

        await RunPostPublishingTask(
            nameof(INotificationsService.NotifySubscribersIfApplicable),
            releaseVersionIds,
            cancellationToken,
            () => notificationsService.NotifySubscribersIfApplicable(releaseVersionIds)
        );

        await RunPostPublishingTask(
            nameof(INotificationsService.SendReleasePublishingFeedbackEmails),
            releaseVersionIds,
            cancellationToken,
            () => notificationsService.SendReleasePublishingFeedbackEmails(releaseVersionIds)
        );

        await RunPostPublishingTask(
            nameof(IEducationInNumbersService.UpdateEinTiles),
            releaseVersionIds,
            cancellationToken,
            () => educationInNumbersService.UpdateEinTiles(releaseVersionIds, cancellationToken)
        );
    }

    private async Task RunPostPublishingTask(
        string taskName,
        Guid[] releaseVersionIds,
        CancellationToken cancellationToken,
        Func<Task> task
    )
    {
        try
        {
            await task();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Publishing was intentionally cancelled, so don't treat this as a task failure
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Post-publishing task {TaskName} failed for release versions {ReleaseVersionIds}",
                taskName,
                string.Join(", ", releaseVersionIds)
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
