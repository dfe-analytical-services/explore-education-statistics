using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class PublishingCompletionService : IPublishingCompletionService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IContentService _contentService;
    private readonly INotificationsService _notificationsService;
    private readonly IReleasePublishingStatusService _releasePublishingStatusService;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly ILogger<PublishingCompletionService> _logger;

    public PublishingCompletionService(
        ContentDbContext contentDbContext,
        IContentService contentService,
        INotificationsService notificationsService,
        IReleasePublishingStatusService releasePublishingStatusService, 
        IPublicationCacheService publicationCacheService,
        ILogger<PublishingCompletionService> logger)
    {
        _contentDbContext = contentDbContext;
        _contentService = contentService;
        _notificationsService = notificationsService;
        _releasePublishingStatusService = releasePublishingStatusService;
        _publicationCacheService = publicationCacheService;
        _logger = logger;
    }

    public async Task CompletePublishingIfAllStagesComplete(Guid releaseId, Guid releaseStatusId)
    {
        var releaseStatus = await _releasePublishingStatusService.GetLatestAsync(releaseId);

        if (releaseStatus.AllStagesPriorToPublishingComplete())
        {
            var release = await _contentDbContext.Releases
                .SingleAsync(r => r.Id == releaseId);

            // Update the cached publication and any cached superseded publications.
            // If this is the first live release of the publication, the superseding is now enforced
            var publicationsToUpdate = await _contentDbContext.Publications
                .Where(p => p.Id == release.PublicationId || p.SupersededById == release.PublicationId)
                .ToListAsync();

            await publicationsToUpdate
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    publication => _publicationCacheService.UpdatePublication(publication.Slug));

            await _contentService.DeletePreviousVersionsDownloadFiles(releaseId);
            await _contentService.DeletePreviousVersionsContent(releaseId);

            await _notificationsService.NotifySubscribersIfApplicable(releaseId);

            // Update the cached trees in case any methodologies/publications
            // are now accessible for the first time after publishing these releases
            await _contentService.UpdateCachedTaxonomyBlobs();

            _logger.LogInformation("Publishing of Release {ReleaseId} complete", releaseId);
        }
    }
}