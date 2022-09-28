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

public class PublishingCompletionService : IPublishingCompletionService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IContentService _contentService;
    private readonly INotificationsService _notificationsService;
    private readonly IReleasePublishingStatusService _releasePublishingStatusService;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IReleaseService _releaseService;

    public PublishingCompletionService(
        ContentDbContext contentDbContext,
        IContentService contentService,
        INotificationsService notificationsService,
        IReleasePublishingStatusService releasePublishingStatusService, 
        IPublicationCacheService publicationCacheService,
        IReleaseService releaseService)
    {
        _contentDbContext = contentDbContext;
        _contentService = contentService;
        _notificationsService = notificationsService;
        _releasePublishingStatusService = releasePublishingStatusService;
        _publicationCacheService = publicationCacheService;
        _releaseService = releaseService;
    }

    public async Task CompletePublishingIfAllPriorStagesComplete(
        IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releaseAndReleaseStatusIds, 
        DateTime publishedDate)
    {
        var releaseStatuses = await releaseAndReleaseStatusIds
            .ToAsyncEnumerable()
            .SelectAwait(async status => await _releasePublishingStatusService.GetAsync(status.ReleaseId, status.ReleaseStatusId))
            .ToArrayAsync();

        await CompletePublishingIfAllPriorStagesComplete(releaseStatuses, publishedDate);
    }

    public async Task CompletePublishingIfAllPriorStagesComplete(
        ReleasePublishingStatus[] releaseStatuses, 
        DateTime publishedDate)
    {
        var prePublishingStagesComplete = releaseStatuses
            .Where(status => status.AllStagesPriorToPublishingComplete())
            .ToList();
        
        var releaseIdsToUpdate = prePublishingStagesComplete
            .Select(status => status.ReleaseId)
            .ToArray();

        await releaseIdsToUpdate
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(releaseId => _releaseService.SetPublishedDates(releaseId, publishedDate));

        var publicationSlugs = prePublishingStagesComplete
            .Select(status => status.PublicationSlug)
            .Distinct();

        var directlyRelatedPublicationIds = await _contentDbContext
            .Publications
            .Where(p => publicationSlugs.Contains(p.Slug))
            .Select(p => p.Id)
            .ToListAsync();
        
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
        
        await releaseStatuses
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(status => _releasePublishingStatusService
                .UpdatePublishingStageAsync(
                    status.ReleaseId, 
                    status.Id, 
                    ReleasePublishingStatusPublishingStage.Complete));
    }
}