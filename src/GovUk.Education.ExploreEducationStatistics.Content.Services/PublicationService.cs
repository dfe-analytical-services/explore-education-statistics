#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class PublicationService : IPublicationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IMapper _mapper;

    public PublicationService(ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IMapper mapper)
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _mapper = mapper;
    }

    [BlobCache(typeof(PublicationCacheKey))]
    public async Task<Either<ActionResult, CachedPublicationViewModel>> GetViewModel(string publicationSlug)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query => query
                .Include(p => p.Contact)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme)
                .Where(p => p.Slug == publicationSlug))
            .OnSuccess(async publication =>
            {
                var publicationViewModel = _mapper.Map<CachedPublicationViewModel>(publication);
                // NOTE: BlobCache won't cache result if return a Either.IsLeft - if there is no latestRelease
                return await GetLatestRelease(publication.Id)
                    .OnSuccess(latestRelease =>
                    {
                        publicationViewModel.LatestReleaseId = latestRelease.Id;
                        publicationViewModel.Releases = GetReleaseViewModels(publication.Id);
                        return publicationViewModel;
                    });
            });
    }

    private async Task<Either<ActionResult,Release>> GetLatestRelease(Guid publicationId)
    {
        // @MarkFix EES-3149 exclude superseded releases here
        var releases = await _contentDbContext.Releases
            .Include(r => r.Publication)
            .Where(release => release.PublicationId == publicationId)
            .ToListAsync();

        var release = releases
            .Where(IsLatestPublishedVersionOfRelease)
            .OrderBy(release => release.Year)
            .ThenBy(release => release.TimePeriodCoverage)
            .LastOrDefault();

        if (release == null)
        {
            return new NotFoundResult();
        }

        return release;
    }

    private List<ReleaseTitleViewModel> GetReleaseViewModels(Guid publicationId)
    {
        var releases = _contentDbContext.Releases
            .Include(r => r.Publication)
            .Where(release => release.PublicationId == publicationId)
            .ToList()
            .Where(IsLatestPublishedVersionOfRelease)
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage);
        return _mapper.Map<List<ReleaseTitleViewModel>>(releases);
    }

    private static bool IsLatestPublishedVersionOfRelease(Release release)
    {
        if (release.Publication?.Releases == null || !release.Publication.Releases.Any())
        {
            throw new ArgumentException(
                "Release must be hydrated with Publications Releases to test the latest published version");
        }

        return
            // Release itself must be live
            release.Live
            // It must also be the latest version unless the later version is a draft not included for publishing
            && !release.Publication.Releases.Any(r =>
                r.Live
                && r.PreviousVersionId == release.Id
                && r.Id != release.Id);
    }
}
