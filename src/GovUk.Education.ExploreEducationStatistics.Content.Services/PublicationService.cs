#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortDirection;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class PublicationService : IPublicationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IPublicationRepository _publicationRepository;
    private readonly IReleaseRepository _releaseRepository;
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public PublicationService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IPublicationRepository publicationRepository,
        IReleaseRepository releaseRepository,
        IReleaseVersionRepository releaseVersionRepository)
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _publicationRepository = publicationRepository;
        _releaseRepository = releaseRepository;
        _releaseVersionRepository = releaseVersionRepository;
    }

    public async Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetSummary(Guid publicationId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query => query
                .Include(p => p.LatestPublishedReleaseVersion)
                .Where(p => p.Id == publicationId))
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
                    Published = publication.LatestPublishedReleaseVersion!.Published!.Value,
                };
            });
    }

    public async Task<Either<ActionResult, PublicationCacheViewModel>> Get(string publicationSlug)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query => query
                .Include(p => p.ReleaseVersions)
                .Include(p => p.Contact)
                .Include(p => p.Theme)
                .Include(p => p.SupersededBy)
                .Where(p => p.Slug == publicationSlug))
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

    public async Task<IList<PublicationTreeThemeViewModel>> GetPublicationTree()
    {
        var themes = await _contentDbContext.Themes
            .Include(theme => theme.Publications)
            .ThenInclude(publication => publication.SupersededBy)
            .ToListAsync();

        return await themes
            .ToAsyncEnumerable()
            .SelectAwait(async theme => await BuildPublicationTreeTheme(theme))
            .Where(theme => theme.Publications.Any())
            .OrderBy(theme => theme.Title)
            .ToListAsync();
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        ReleaseType? releaseType = null,
        Guid? themeId = null,
        string? search = null,
        PublicationsSortBy? sort = null,
        SortDirection? sortDirection = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Guid>? publicationIds = null)
    {
        sort ??= search == null ? Title : Relevance;
        sortDirection ??= sort == Title ? Asc : Desc;

        // Publications must have a published release and not be superseded
        var baseQueryable = _contentDbContext.Publications
            .Where(p => p.LatestPublishedReleaseVersionId.HasValue &&
                        (p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseVersionId.HasValue));

        // Retrieve only requested publication IDs if specified
        if (publicationIds is not null)
        {
            baseQueryable = baseQueryable.Where(p => publicationIds.Contains(p.Id));
        }

        // Apply release type and theme filters
        if (releaseType.HasValue)
        {
            baseQueryable = baseQueryable.Where(p => p.LatestPublishedReleaseVersion!.Type == releaseType.Value);
        }

        if (themeId.HasValue)
        {
            baseQueryable = baseQueryable.Where(p => p.ThemeId == themeId.Value);
        }

        // Apply a free-text search filter
        var queryable = baseQueryable.JoinFreeText(_contentDbContext.PublicationsFreeTextTable, p => p.Id, search);

        // Apply sorting
        var orderedQueryable = sort switch
        {
            Published =>
                sortDirection == Asc
                    ? queryable
                        .OrderBy(result => DateOnly.FromDateTime(result.Value.LatestPublishedReleaseVersion!.Published!.Value))
                        .ThenByReleaseType()
                    : queryable
                        .OrderByDescending(result => DateOnly.FromDateTime(result.Value.LatestPublishedReleaseVersion!.Published!.Value))
                        .ThenByReleaseType(),
            Relevance =>
                sortDirection == Asc
                    ? queryable.OrderBy(result => result.Rank)
                    : queryable.OrderByDescending(result => result.Rank),
            Title =>
                sortDirection == Asc
                    ? queryable.OrderBy(result => result.Value.Title)
                    : queryable.OrderByDescending(result => result.Value.Title),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, message: null)
        };

        // Then sort by publication id to provide a stable sort order
        orderedQueryable = orderedQueryable.ThenBy(result => result.Value.Id);

        // Get the total results count for the paginated response
        var totalResults = await orderedQueryable.CountAsync();

        // Apply offset pagination and execute the query
        var results = await orderedQueryable
            .Paginate(page, pageSize)
            .Select(result =>
                new PublicationSearchResultViewModel
                {
                    Id = result.Value.Id,
                    Slug = result.Value.Slug,
                    LatestReleaseSlug = result.Value.LatestPublishedReleaseVersion!.Release.Slug,
                    Summary = result.Value.Summary,
                    Title = result.Value.Title,
                    Theme = result.Value.Theme.Title,
                    Published = result.Value.LatestPublishedReleaseVersion!.Published!.Value,
                    Type = result.Value.LatestPublishedReleaseVersion!.Type,
                    Rank = result.Rank
                }).ToListAsync();

        return new PaginatedListViewModel<PublicationSearchResultViewModel>(results, totalResults, page, pageSize);
    }

    private static PublicationCacheViewModel BuildPublicationViewModel(
        Publication publication,
        List<Release> releases,
        bool isSuperseded)
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
                Summary = theme.Summary
            },
            Contact = new ContactViewModel(publication.Contact),
            ExternalMethodology = publication.ExternalMethodology != null
                ? new ExternalMethodologyViewModel(publication.ExternalMethodology)
                : null,
            LatestReleaseId = publication.LatestPublishedReleaseVersionId!.Value,
            IsSuperseded = isSuperseded,
            SupersededBy = isSuperseded
                ? new PublicationSupersededByViewModel
                {
                    Id = publication.SupersededBy!.Id,
                    Slug = publication.SupersededBy.Slug,
                    Title = publication.SupersededBy.Title
                }
                : null,
            Releases = releases
                .Select(r => new ReleaseTitleViewModel
                {
                    Id = r.Id,
                    Slug = r.Slug,
                    Title = r.Title
                })
                .ToList(),
            ReleaseSeries = releaseSeriesItemViewModels
        };
    }

    private static List<ReleaseSeriesItemViewModel> BuildReleaseSeriesItemViewModels(
        Publication publication,
        List<Release> releases)
    {
        var publishedReleasesById = releases.ToDictionary(r => r.Id);
        return publication.ReleaseSeries
            // Only include release series items for legacy links and published releases
            .Where(rsi => rsi.IsLegacyLink || publishedReleasesById.ContainsKey(rsi.ReleaseId!.Value))
            .Select(rsi =>
            {
                if (rsi.IsLegacyLink)
                {
                    return new ReleaseSeriesItemViewModel
                    {
                        Description = rsi.LegacyLinkDescription!,
                        LegacyLinkUrl = rsi.LegacyLinkUrl
                    };
                }

                var release = publishedReleasesById[rsi.ReleaseId!.Value];

                return new ReleaseSeriesItemViewModel
                {
                    Description = release.Title,
                    ReleaseId = release.Id,
                    ReleaseSlug = release.Slug
                };
            })
            .ToList();
    }

    private async Task<PublicationTreeThemeViewModel> BuildPublicationTreeTheme(Theme theme)
    {
        var publications = await theme.Publications
            .Where(publication => publication.LatestPublishedReleaseVersionId != null)
            .ToAsyncEnumerable()
            .SelectAwait(async publication =>
                await BuildPublicationTreePublication(publication))
            .OrderBy(publication => publication.Title)
            .ToListAsync();

        return new PublicationTreeThemeViewModel
        {
            Id = theme.Id,
            Title = theme.Title,
            Summary = theme.Summary,
            Publications = publications
        };
    }

    private async Task<PublicationTreePublicationViewModel> BuildPublicationTreePublication(Publication publication)
    {
        var isSuperseded = await _publicationRepository.IsSuperseded(publication.Id);

        var latestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId;
        var latestReleaseHasData =
            latestPublishedReleaseVersionId.HasValue && await HasAnyDataFiles(latestPublishedReleaseVersionId.Value);

        var publishedReleaseVersionIds =
            await _releaseVersionRepository.ListLatestReleaseVersionIds(publication.Id, publishedOnly: true);
        var anyLiveReleaseHasData = await publishedReleaseVersionIds
            .ToAsyncEnumerable()
            .AnyAwaitAsync(async id => await HasAnyDataFiles(id));

        return new PublicationTreePublicationViewModel
        {
            Id = publication.Id,
            Title = publication.Title,
            Slug = publication.Slug,
            IsSuperseded = isSuperseded,
            SupersededBy = isSuperseded
                ? new PublicationSupersededByViewModel
                {
                    Id = publication.SupersededBy!.Id,
                    Slug = publication.SupersededBy.Slug,
                    Title = publication.SupersededBy.Title
                }
                : null,
            LatestReleaseHasData = latestReleaseHasData,
            AnyLiveReleaseHasData = anyLiveReleaseHasData
        };
    }

    private async Task<bool> HasAnyDataFiles(Guid releaseVersionId)
    {
        return await _contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .AnyAsync(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data);
    }

    public async Task<IList<PublicationInfoViewModel>> ListPublicationInfos(
        Guid? themeId = null,
        CancellationToken cancellationToken = default) =>
        await _contentDbContext.Publications
            .Include(p => p.LatestPublishedReleaseVersion)
            .ThenInclude(rv => rv!.Release)
            .Where(p => 
                // Is published
                p.LatestPublishedReleaseVersionId.HasValue 
                // Is not superseded/archived
                && (p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseVersionId.HasValue))
            .If(!themeId.IsBlank())
                .ThenWhere(p => p.ThemeId == themeId!.Value)
            .Select(publication => PublicationInfoViewModel.FromEntity(publication))
            .ToListAsync(cancellationToken);
}
