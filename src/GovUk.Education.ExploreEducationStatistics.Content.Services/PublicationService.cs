#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService.
    PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class PublicationService : IPublicationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IPublicationRepository _publicationRepository;
    private readonly IUserService _userService;

    public PublicationService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IPublicationRepository publicationRepository,
        IUserService userService)
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _publicationRepository = publicationRepository;
        _userService = userService;
    }

    public async Task<Either<ActionResult, PublicationCacheViewModel>> Get(string publicationSlug)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(query => query
                .Include(p => p.Releases)
                .Include(p => p.Contact)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme)
                .Where(p => p.Slug == publicationSlug))
            .OnSuccess(_userService.CheckCanViewPublication)
            .OnSuccess(async publication =>
            {
                var isSuperseded = await _publicationRepository.IsSuperseded(publication.Id);
                return BuildPublicationViewModel(publication, isSuperseded);
            });
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        ReleaseType? releaseType = null,
        Guid? themeId = null,
        string? search = null,
        PublicationsSortBy? sort = null,
        SortOrder? order = null,
        int page = 1,
        int pageSize = 10)
    {
        sort ??= search == null ? Title : Relevance;
        order ??= order ?? (sort == Title ? Asc : Desc);

        // Publications must have a published release and not be superseded
        var baseQueryable = _contentDbContext.Publications
            .Where(p => p.LatestPublishedReleaseId.HasValue &&
                        (p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseId.HasValue));

        // Apply release type and theme filters
        if (releaseType.HasValue)
        {
            baseQueryable = baseQueryable.Where(p => p.LatestPublishedRelease!.Type == releaseType.Value);
        }

        if (themeId.HasValue)
        {
            baseQueryable = baseQueryable.Where(p => p.Topic.ThemeId == themeId.Value);
        }

        // Apply a free-text search filter
        var queryable = search == null
            ? baseQueryable.Select(publication => new { Publication = publication, Rank = 0 })
            : baseQueryable.Join(_contentDbContext.PublicationsFreeTextTable(search),
                publication => publication.Id,
                freeTextRank => freeTextRank.Id,
                (publication, freeTextRank) => new { Publication = publication, freeTextRank.Rank });

        // Apply sorting
        var orderedQueryable = sort switch
        {
            Published =>
                order == Asc
                    ? queryable.OrderBy(p => p.Publication.LatestPublishedRelease!.Published)
                    : queryable.OrderByDescending(p => p.Publication.LatestPublishedRelease!.Published),
            Relevance =>
                order == Asc
                    ? queryable.OrderBy(p => p.Rank)
                    : queryable.OrderByDescending(p => p.Rank),
            Title =>
                order == Asc
                    ? queryable.OrderBy(p => p.Publication.Title)
                    : queryable.OrderByDescending(p => p.Publication.Title),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, message: null)
        };

        // Then sort by publication id to provide a stable sort order
        orderedQueryable = orderedQueryable.ThenBy(p => p.Publication.Id);

        // Get the total results count for the paginated response
        var totalResults = await orderedQueryable.CountAsync();

        // Apply offset pagination and execute the query
        var results = await orderedQueryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(tuple =>
                new PublicationSearchResultViewModel
                {
                    Id = tuple.Publication.Id,
                    Slug = tuple.Publication.Slug,
                    Summary = tuple.Publication.Summary,
                    Title = tuple.Publication.Title,
                    Theme = tuple.Publication.Topic.Theme.Title,
                    Published = tuple.Publication.LatestPublishedRelease!.Published!.Value,
                    Type = tuple.Publication.LatestPublishedRelease!.Type,
                    Rank = tuple.Rank
                }).ToListAsync();

        return new PaginatedListViewModel<PublicationSearchResultViewModel>(results, totalResults, page, pageSize);
    }

    private static PublicationCacheViewModel BuildPublicationViewModel(
        Publication publication,
        bool isSuperseded)
    {
        return new PublicationCacheViewModel
        {
            Id = publication.Id,
            Title = publication.Title,
            Slug = publication.Slug,
            LegacyReleases = publication.LegacyReleases
                .OrderByDescending(legacyRelease => legacyRelease.Order)
                .Select(legacyRelease => new LegacyReleaseViewModel(legacyRelease))
                .ToList(),
            Topic = new TopicViewModel(new ThemeViewModel(publication.Topic.Theme.Title)),
            Contact = new ContactViewModel(publication.Contact),
            ExternalMethodology = publication.ExternalMethodology != null
                ? new ExternalMethodologyViewModel(publication.ExternalMethodology)
                : null,
            LatestReleaseId = publication.LatestPublishedReleaseId!.Value,
            IsSuperseded = isSuperseded,
            Releases = ListPublishedReleases(publication)
        };
    }

    private static List<ReleaseTitleViewModel> ListPublishedReleases(Publication publication)
    {
        return publication.GetPublishedReleases()
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage)
            .Select(release => new ReleaseTitleViewModel
            {
                Id = release.Id,
                Slug = release.Slug,
                Title = release.Title
            })
            .ToList();
    }
}
