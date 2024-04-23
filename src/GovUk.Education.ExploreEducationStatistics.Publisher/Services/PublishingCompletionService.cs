#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class PublishingCompletionService : IPublishingCompletionService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IContentService _contentService;
    private readonly IMethodologyService _methodologyService;
    private readonly INotificationsService _notificationsService;
    private readonly IReleasePublishingStatusService _releasePublishingStatusService;
    private readonly IReleaseVersionRepository _releaseVersionRepository;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IReleaseService _releaseService;
    private readonly IRedirectsCacheService _redirectsCacheService;
    private readonly IDataSetPublishingService _dataSetPublishingService;

    public PublishingCompletionService(
        ContentDbContext contentDbContext,
        IContentService contentService,
        IMethodologyService methodologyService,
        INotificationsService notificationsService,
        IReleasePublishingStatusService releasePublishingStatusService,
        IPublicationCacheService publicationCacheService,
        IReleaseVersionRepository releaseVersionRepository,
        IReleaseService releaseService,
        IRedirectsCacheService redirectsCacheService,
        IDataSetPublishingService dataSetPublishingService)
    {
        _contentDbContext = contentDbContext;
        _contentService = contentService;
        _methodologyService = methodologyService;
        _notificationsService = notificationsService;
        _releasePublishingStatusService = releasePublishingStatusService;
        _publicationCacheService = publicationCacheService;
        _releaseVersionRepository = releaseVersionRepository;
        _releaseService = releaseService;
        _redirectsCacheService = redirectsCacheService;
        _dataSetPublishingService = dataSetPublishingService;
    }

    public async Task CompletePublishingIfAllPriorStagesComplete(
        IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releaseVersionAndReleaseStatusIds)
    {
        var releaseStatuses = await releaseVersionAndReleaseStatusIds
            .ToAsyncEnumerable()
            .SelectAwait(async status =>
                await _releasePublishingStatusService.GetAsync(releaseVersionId: status.ReleaseVersionId,
                    releaseStatusId: status.ReleaseStatusId))
            .ToArrayAsync();

        await CompletePublishingIfAllPriorStagesComplete(releaseStatuses);
    }

    public async Task CompletePublishingIfAllPriorStagesComplete(
        ReleasePublishingStatus[] releaseStatuses)
    {
        var prePublishingStagesComplete = releaseStatuses
            .Where(status => status.AllStagesPriorToPublishingComplete())
            .ToList();

        if (!prePublishingStagesComplete.Any())
        {
            return;
        }

        await prePublishingStagesComplete
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(status => _releasePublishingStatusService
                .UpdatePublishingStageAsync(
                    releaseVersionId: status.ReleaseVersionId,
                    releaseStatusId: status.Id,
                    ReleasePublishingStatusPublishingStage.Started));

        var releaseVersionIdsToUpdate = prePublishingStagesComplete
            .Select(status => status.ReleaseVersionId)
            .ToArray();

        await releaseVersionIdsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(releaseId => _releaseService.CompletePublishing(releaseId, DateTime.UtcNow));

        await releaseVersionIdsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async releaseId =>
            {
                var releaseVersion = await _releaseService.Get(releaseId);
                var methodologyVersions =
                    await _methodologyService.GetLatestVersionByRelease(releaseVersion);

                if (!methodologyVersions.Any())
                {
                    return;
                }

                foreach (var methodologyVersion in methodologyVersions)
                {
                    // WARN: This must be called before PublicationRepository#UpdateLatestPublishedRelease
                    if (await _methodologyService.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
                    {
                        await _methodologyService.Publish(methodologyVersion);
                    }
                }
            });

        var directlyRelatedPublicationIds = await _contentDbContext
            .ReleaseVersions
            .Where(rv => releaseVersionIdsToUpdate.Contains(rv.Id))
            .Select(rv => rv.PublicationId)
            .Distinct()
            .ToListAsync();

        await directlyRelatedPublicationIds
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(UpdateLatestPublishedRelease);

        // Update the cached publication and any cached superseded publications.
        // If this is the first live release of the publication, the superseding is now enforced
        var publicationSlugsToUpdate = await _contentDbContext
            .Publications
            .Where(p => directlyRelatedPublicationIds.Contains(p.Id) ||
                        (p.SupersededById != null && directlyRelatedPublicationIds.Contains(p.SupersededById.Value)))
            .Select(p => p.Slug)
            .ToListAsync();

        await publicationSlugsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(publicationSlug => _publicationCacheService.UpdatePublication(publicationSlug));

        await _contentService.DeletePreviousVersionsDownloadFiles(releaseVersionIdsToUpdate);
        await _contentService.DeletePreviousVersionsContent(releaseVersionIdsToUpdate);

        await _notificationsService.NotifySubscribersIfApplicable(releaseVersionIdsToUpdate);

        // Update the cached trees in case any methodologies/publications
        // are now accessible for the first time after publishing these releases
        await _contentService.UpdateCachedTaxonomyBlobs();

        await _redirectsCacheService.UpdateRedirects();

        await _dataSetPublishingService.PublishDataSets(releaseVersionIdsToUpdate);

        await prePublishingStagesComplete
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(status => _releasePublishingStatusService
                .UpdatePublishingStageAsync(
                    releaseVersionId: status.ReleaseVersionId,
                    releaseStatusId: status.Id,
                    ReleasePublishingStatusPublishingStage.Complete));
    }

    private async Task UpdateLatestPublishedRelease(Guid publicationId)
    {
        var publication = await _contentDbContext.Publications
            .SingleAsync(p => p.Id == publicationId);

        var latestPublishedReleaseVersion = await _releaseVersionRepository.GetLatestPublishedReleaseVersion(publicationId);
        publication.LatestPublishedReleaseVersionId = latestPublishedReleaseVersion!.Id;

        _contentDbContext.Update(publication);
        await _contentDbContext.SaveChangesAsync();
    }
}
