#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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

    public PublicationService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IPublicationRepository publicationRepository)
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _publicationRepository = publicationRepository;
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
                .Include(p => p.SupersededBy)
                .Where(p => p.Slug == publicationSlug))
            .OnSuccess(async publication =>
            {
                if (publication.LatestPublishedReleaseId == null)
                {
                    return new Either<ActionResult, PublicationCacheViewModel>(new NotFoundResult());
                }

                var isSuperseded = await _publicationRepository.IsSuperseded(publication.Id);
                return BuildPublicationViewModel(publication, isSuperseded);
            });
    }

    public async Task<IList<PublicationTreeThemeViewModel>> GetPublicationTree()
    {
        var themes = await _contentDbContext.Themes
            .Include(theme => theme.Topics)
            .ThenInclude(topic => topic.Publications)
            .ThenInclude(publication => publication.Releases)
        
            .Include(theme => theme.Topics)
            .ThenInclude(topic => topic.Publications)
            .ThenInclude(publication => publication.SupersededBy)
            .ToListAsync();

        return await themes
            .ToAsyncEnumerable()
            .SelectAwait(async theme => await BuildPublicationTreeTheme(theme))
            .Where(theme => theme.Topics.Any())
            .OrderBy(theme => theme.Title)
            .ToListAsync();
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
        var topic = new TopicViewModel(new ThemeViewModel(
            publication.Topic.Theme.Id,
            Slug: publication.Topic.Theme.Slug,
            Title: publication.Topic.Theme.Title,
            Summary: publication.Topic.Theme.Summary
        ));

        return new PublicationCacheViewModel
        {
            Id = publication.Id,
            Title = publication.Title,
            Slug = publication.Slug,
            LegacyReleases = publication.LegacyReleases
                .OrderByDescending(legacyRelease => legacyRelease.Order)
                .Select(legacyRelease => new LegacyReleaseViewModel(legacyRelease))
                .ToList(),
            Topic = topic,
            Contact = new ContactViewModel(publication.Contact),
            ExternalMethodology = publication.ExternalMethodology != null
                ? new ExternalMethodologyViewModel(publication.ExternalMethodology)
                : null,
            LatestReleaseId = publication.LatestPublishedReleaseId!.Value,
            IsSuperseded = isSuperseded,
            SupersededBy = isSuperseded
                ? new PublicationSupersededByViewModel
                {
                    Id = publication.SupersededBy!.Id,
                    Slug = publication.SupersededBy.Slug,
                    Title = publication.SupersededBy.Title
                }
                : null,
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

    private async Task<PublicationTreeThemeViewModel> BuildPublicationTreeTheme(Theme theme)
    {
        var topics = await theme.Topics
            .ToAsyncEnumerable()
            .SelectAwait(async topic => await BuildPublicationTreeTopic(topic))
            .Where(topic => topic.Publications.Any())
            .OrderBy(topic => topic.Title)
            .ToListAsync();

        return new PublicationTreeThemeViewModel
        {
            Id = theme.Id,
            Title = theme.Title,
            Summary = theme.Summary,
            Topics = topics
        };
    }

    private async Task<PublicationTreeTopicViewModel> BuildPublicationTreeTopic(Topic topic)
    {
        var publications = await topic.Publications
            .Where(publication => publication.LatestPublishedReleaseId != null)
            .ToAsyncEnumerable()
            .SelectAwait(async publication =>
                await BuildPublicationTreePublication(publication))
            .OrderBy(publication => publication.Title)
            .ToListAsync();

        return new PublicationTreeTopicViewModel
        {
            Id = topic.Id,
            Title = topic.Title,
            Publications = publications
        };
    }

    private async Task<PublicationTreePublicationViewModel> BuildPublicationTreePublication(Publication publication)
    {
        var latestPublishedReleaseId = publication.LatestPublishedReleaseId;
        var isSuperseded = await _publicationRepository.IsSuperseded(publication.Id);
        
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
            LatestReleaseHasData = latestPublishedReleaseId != null &&
                                   await HasAnyDataFiles(latestPublishedReleaseId.Value),
            AnyLiveReleaseHasData = await publication.Releases
                .ToAsyncEnumerable()
                .AnyAwaitAsync(async r => r.IsLatestPublishedVersionOfRelease()
                                          && await HasAnyDataFiles(r.Id))
        };
    }

    private async Task<bool> HasAnyDataFiles(Guid releaseId)
    {
        return await _contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .AnyAsync(rf => rf.ReleaseId == releaseId && rf.File.Type == FileType.Data);
    }
}