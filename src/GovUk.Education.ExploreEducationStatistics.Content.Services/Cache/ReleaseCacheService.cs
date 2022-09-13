#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class ReleaseCacheService : IReleaseCacheService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IMethodologyCacheService _methodologyCacheService;
    private readonly IPublicationCacheService _publicationCacheService;

    public ReleaseCacheService(IBlobStorageService blobStorageService,
        IMethodologyCacheService methodologyCacheService,
        IPublicationCacheService publicationCacheService)
    {
        _blobStorageService = blobStorageService;
        _methodologyCacheService = methodologyCacheService;
        _publicationCacheService = publicationCacheService;
    }

    public async Task<Either<ActionResult, CachedReleaseViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug = null)
    {
        var releasePath = releaseSlug != null
            ? PublicContentReleasePath(publicationSlug, releaseSlug)
            : PublicContentLatestReleasePath(publicationSlug);
        try
        {
            return await _blobStorageService.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                releasePath
            ) ?? new Either<ActionResult, CachedReleaseViewModel>(new NotFoundResult());
        }
        catch (FileNotFoundException)
        {
            return new NotFoundResult();
        }
    }

    public Task<Either<ActionResult, ReleaseSummaryViewModel>> GetReleaseSummary(string publicationSlug,
        string? releaseSlug = null)
    {
        return _publicationCacheService.GetPublication(publicationSlug)
            .OnSuccessCombineWith(_ => GetRelease(publicationSlug, releaseSlug))
            .OnSuccess(publicationAndRelease =>
            {
                var (publication, release) = publicationAndRelease;
                return new ReleaseSummaryViewModel(
                    release,
                    publication
                );
            });
    }

    public Task<Either<ActionResult, ReleaseViewModel>> GetReleaseAndPublication(string publicationSlug,
        string? releaseSlug = null)
    {
        return _publicationCacheService.GetPublication(publicationSlug)
            .OnSuccessCombineWith(publication => _methodologyCacheService.GetSummariesByPublication(publication.Id))
            .OnSuccess(async publicationAndMethodologies =>
            {
                var (publication, methodologies) = publicationAndMethodologies;
                return await GetRelease(publicationSlug, releaseSlug)
                    .OnSuccess(release => new ReleaseViewModel(
                        release,
                        publication with
                        {
                            Releases = publication.Releases
                                .Where(releaseTitleViewModel => releaseTitleViewModel.Id != release.Id)
                                .ToList(),
                            Methodologies = methodologies
                        }
                    ));
            });
    }
}
