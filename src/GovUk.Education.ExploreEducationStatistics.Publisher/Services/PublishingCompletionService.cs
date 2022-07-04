using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class PublishingCompletionService : IPublishingCompletionService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IBlobCacheService _blobCacheService;
    private readonly IContentService _contentService;
    private readonly INotificationsService _notificationsService;
    private readonly IReleaseService _releaseService;
    private readonly IReleasePublishingStatusService _releasePublishingStatusService;
    private readonly IPublishingService _publishingService;

    public PublishingCompletionService(
        ContentDbContext contentDbContext,
        IBlobCacheService blobCacheService,
        IContentService contentService,
        INotificationsService notificationsService,
        IReleaseService releaseService,
        IReleasePublishingStatusService releasePublishingStatusService, 
        IPublishingService publishingService)
    {
        _contentDbContext = contentDbContext;
        _blobCacheService = blobCacheService;
        _contentService = contentService;
        _notificationsService = notificationsService;
        _releaseService = releaseService;
        _releasePublishingStatusService = releasePublishingStatusService;
        _publishingService = publishingService;
    }
    
    

    public async Task CompletePublishingIfAllStagesComplete(Guid releaseId, Guid releaseStatusId)
    {
        var releaseStatus = await _releasePublishingStatusService.GetLatestAsync(releaseId);
        if (releaseStatus.AllStagesPriorToPublishingComplete())
        {
            // Invalidate the cached trees in case any methodologies/publications
            // are now accessible for the first time after publishing these releases
            await _contentService.DeleteCachedTaxonomyBlobs();

            // Invalidate publication cache for release
            var release = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(r => r.Id == releaseId)
                .SingleAsync();
            await _blobCacheService.DeleteItem(new PublicationCacheKey(release.Publication.Slug));

            // Invalidate publication cache for superseded publications, as potentially affected. If newly
            // published release is first Live release for the publication, the superseding is now enforced
            await _contentDbContext.Publications
                .Where(p => p.SupersededById == release.Publication.Id)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(publication =>
                    _blobCacheService.DeleteItem(new PublicationCacheKey(publication.Slug)));

            await _contentService.DeletePreviousVersionsDownloadFiles(releaseId);
            await _contentService.DeletePreviousVersionsContent(releaseId);

            await _releaseService.SetPublishedDates(releaseId, DateTime.UtcNow);
            await _notificationsService.NotifySubscribersIfApplicable(releaseId);

        }
    }
}