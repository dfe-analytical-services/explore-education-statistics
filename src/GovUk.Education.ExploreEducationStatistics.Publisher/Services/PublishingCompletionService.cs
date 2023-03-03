#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
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
    private readonly IPublicationRepository _publicationRepository;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IReleaseService _releaseService;

    public PublishingCompletionService(ContentDbContext contentDbContext,
        IContentService contentService,
        IMethodologyService methodologyService,
        INotificationsService notificationsService,
        IReleasePublishingStatusService releasePublishingStatusService,
        IPublicationRepository publicationRepository,
        IPublicationCacheService publicationCacheService,
        IReleaseService releaseService)
    {
        _contentDbContext = contentDbContext;
        _contentService = contentService;
        _methodologyService = methodologyService;
        _notificationsService = notificationsService;
        _releasePublishingStatusService = releasePublishingStatusService;
        _publicationCacheService = publicationCacheService;
        _releaseService = releaseService;
        _publicationRepository = publicationRepository;
    }

    public async Task CompletePublishingIfAllPriorStagesComplete(
        IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releaseAndReleaseStatusIds)
    {
        var releaseStatuses = await releaseAndReleaseStatusIds
            .ToAsyncEnumerable()
            .SelectAwait(async status => await _releasePublishingStatusService.GetAsync(status.ReleaseId, status.ReleaseStatusId))
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
                    status.ReleaseId,
                    status.Id,
                    ReleasePublishingStatusPublishingStage.Started));

        var releaseIdsToUpdate = prePublishingStagesComplete
            .Select(status => status.ReleaseId)
            .ToArray();

        await releaseIdsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(releaseId => _releaseService.SetPublishedDate(releaseId, DateTime.UtcNow));

        var publicationSlugs = prePublishingStagesComplete
            .Select(status => status.PublicationSlug)
            .Distinct();

        var directlyRelatedPublicationIds = await _contentDbContext
            .Publications
            .Where(p => publicationSlugs.Contains(p.Slug))
            .Select(p => p.Id)
            .ToListAsync();

        await directlyRelatedPublicationIds
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(_publicationRepository.UpdateLatestPublishedRelease);

        // Set the published date on any methodologies used by these publications that are now publicly accessible
        // as a result of releases being published
        await directlyRelatedPublicationIds
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(publicationId =>
                _methodologyService.SetPublishedDatesIfApplicable(publicationId));

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

        await _contentService.DeletePreviousVersionsDownloadFiles(releaseIdsToUpdate);
        await _contentService.DeletePreviousVersionsContent(releaseIdsToUpdate);

        await _notificationsService.NotifySubscribersIfApplicable(releaseIdsToUpdate);

        // Update the cached trees in case any methodologies/publications
        // are now accessible for the first time after publishing these releases
        await _contentService.UpdateCachedTaxonomyBlobs();

        await prePublishingStagesComplete
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(status => _releasePublishingStatusService
                .UpdatePublishingStageAsync(
                    status.ReleaseId,
                    status.Id,
                    ReleasePublishingStatusPublishingStage.Complete));
    }
}
