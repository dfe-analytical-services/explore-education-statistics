#nullable enable
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
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IMapper _mapper;

    public PublicationService(
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IMapper mapper)
    {
        _contentPersistenceHelper = contentPersistenceHelper;
        _mapper = mapper;
    }

    [BlobCache(typeof(PublicationCacheKey))]
    public async Task<Either<ActionResult, PublicationViewModel>> Get(string publicationSlug)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query => query
                .Include(p => p.Releases)
                .Include(p => p.Contact)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme)
                .Where(p => p.Slug == publicationSlug))
            .OnSuccess(publication =>
            {
                var publicationViewModel = _mapper.Map<PublicationViewModel>(publication);
                // NOTE: BlobCache won't cache result if return a Either.IsLeft - if there is no latestRelease
                return GetLatestRelease(publication)
                    .OnSuccess(latestRelease =>
                    {
                        publicationViewModel.LatestReleaseId = latestRelease.Id;
                        publicationViewModel.Releases = ListPublishedReleases(publication);
                        return publicationViewModel;
                    });
            });
    }

    private static Either<ActionResult, Release> GetLatestRelease(Publication publication)
    {
        // @MarkFix EES-3149 exclude superseded releases here
        return publication.LatestPublishedRelease() ?? new Either<ActionResult, Release>(new NotFoundResult());
    }

    private List<ReleaseTitleViewModel> ListPublishedReleases(Publication publication)
    {
        var releases = publication.GetPublishedReleases()
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage)
            .ToList();
        return _mapper.Map<List<ReleaseTitleViewModel>>(releases);
    }
}
