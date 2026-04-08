using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class PublicationService : IPublicationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IPublicationRepository _publicationRepository;
    private readonly IReleaseRepository _releaseRepository;

    public PublicationService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IPublicationRepository publicationRepository,
        IReleaseRepository releaseRepository
    )
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _publicationRepository = publicationRepository;
        _releaseRepository = releaseRepository;
    }

    public async Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetSummary(Guid publicationId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query =>
                query.Include(p => p.LatestPublishedReleaseVersion).Where(p => p.Id == publicationId)
            )
            .OnSuccess(publication =>
            {
                if (publication.LatestPublishedReleaseVersionId == null)
                {
                    return new Either<ActionResult, PublishedPublicationSummaryViewModel>(new NotFoundResult());
                }

                return new PublishedPublicationSummaryViewModel
                {
                    Id = publication.Id,
                    Title = publication.Title,
                    Slug = publication.Slug,
                    Summary = publication.Summary,
                    Published = publication.LatestPublishedReleaseVersion!.PublishedDisplayDate!.Value,
                };
            });
    }

    public async Task<Either<ActionResult, PublicationCacheViewModel>> Get(string publicationSlug)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query =>
                query
                    .Include(p => p.ReleaseVersions)
                    .Include(p => p.Contact)
                    .Include(p => p.Theme)
                    .Include(p => p.SupersededBy)
                    .Where(p => p.Slug == publicationSlug)
            )
            .OnSuccess(async publication =>
            {
                if (publication.LatestPublishedReleaseVersionId == null)
                {
                    return new Either<ActionResult, PublicationCacheViewModel>(new NotFoundResult());
                }

                var publishedReleases = await _releaseRepository.ListPublishedReleases(publication.Id);
                var isSuperseded = await _publicationRepository.IsSuperseded(publication.Id);
                return BuildPublicationViewModel(publication, publishedReleases, isSuperseded);
            });
    }

    private static PublicationCacheViewModel BuildPublicationViewModel(
        Publication publication,
        List<Release> releases,
        bool isSuperseded
    )
    {
        var theme = publication.Theme;

        var releaseSeriesItemViewModels = BuildReleaseSeriesItemViewModels(publication, releases);

        return new PublicationCacheViewModel
        {
            Id = publication.Id,
            Title = publication.Title,
            Summary = publication.Summary,
            Slug = publication.Slug,
            Theme = new ThemeViewModel
            {
                Id = theme.Id,
                Slug = theme.Slug,
                Title = theme.Title,
                Summary = theme.Summary,
            },
            Contact = new ContactViewModel(publication.Contact),
            ExternalMethodology =
                publication.ExternalMethodology != null
                    ? new ExternalMethodologyViewModel(publication.ExternalMethodology)
                    : null,
            LatestReleaseId = publication.LatestPublishedReleaseVersionId!.Value,
            IsSuperseded = isSuperseded,
            SupersededBy = isSuperseded
                ? new PublicationSupersededByViewModel
                {
                    Id = publication.SupersededBy!.Id,
                    Slug = publication.SupersededBy.Slug,
                    Title = publication.SupersededBy.Title,
                }
                : null,
            Releases = releases
                .Select(r => new ReleaseTitleViewModel
                {
                    Id = r.Id,
                    Slug = r.Slug,
                    Title = r.Title,
                })
                .ToList(),
            ReleaseSeries = releaseSeriesItemViewModels,
        };
    }

    private static List<ReleaseSeriesItemViewModel> BuildReleaseSeriesItemViewModels(
        Publication publication,
        List<Release> releases
    )
    {
        var publishedReleasesById = releases.ToDictionary(r => r.Id);
        return publication
            .ReleaseSeries
            // Only include release series items for legacy links and published releases
            .Where(rsi => rsi.IsLegacyLink || publishedReleasesById.ContainsKey(rsi.ReleaseId!.Value))
            .Select(rsi =>
            {
                if (rsi.IsLegacyLink)
                {
                    return new ReleaseSeriesItemViewModel
                    {
                        Description = rsi.LegacyLinkDescription!,
                        LegacyLinkUrl = rsi.LegacyLinkUrl,
                    };
                }

                var release = publishedReleasesById[rsi.ReleaseId!.Value];

                return new ReleaseSeriesItemViewModel
                {
                    Description = release.Title,
                    ReleaseId = release.Id,
                    ReleaseSlug = release.Slug,
                };
            })
            .ToList();
    }

    public async Task<IList<PublicationInfoViewModel>> ListPublicationInfos(
        Guid? themeId = null,
        CancellationToken cancellationToken = default
    ) =>
        await _contentDbContext
            .Publications.Include(p => p.LatestPublishedReleaseVersion)
                .ThenInclude(rv => rv!.Release)
            .Where(p =>
                // Is published
                p.LatestPublishedReleaseVersionId.HasValue
                // Is not superseded/archived
                && (p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseVersionId.HasValue)
            )
            .If(!themeId.IsBlank())
            .ThenWhere(p => p.ThemeId == themeId!.Value)
            .Select(publication => PublicationInfoViewModel.FromEntity(publication))
            .ToListAsync(cancellationToken);
}
