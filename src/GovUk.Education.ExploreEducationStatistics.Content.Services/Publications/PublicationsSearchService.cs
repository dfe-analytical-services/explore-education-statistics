using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortDirection;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationsSearchService(ContentDbContext contentDbContext) : IPublicationsSearchService
{
    public async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> GetPublications(
        ReleaseType? releaseType = null,
        Guid? themeId = null,
        string? search = null,
        PublicationsSortBy? sort = null,
        SortDirection? sortDirection = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Guid>? publicationIds = null,
        CancellationToken cancellationToken = default
    )
    {
        sort ??= search == null ? Title : Relevance;
        sortDirection ??= sort == Title ? Asc : Desc;

        // Publications must have a published release and not be superseded
        var baseQueryable = contentDbContext
            .Publications.WhereHasPublishedRelease()
            .Where(p => p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseVersionId.HasValue);

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
        var queryable = baseQueryable.JoinFreeText(contentDbContext.PublicationsFreeTextTable, p => p.Id, search);

        // Apply sorting
        var orderedQueryable = sort switch
        {
            Published => sortDirection == Asc
                ? queryable
                    .OrderBy(result =>
                        DateOnly.FromDateTime(result.Value.LatestPublishedReleaseVersion!.Published!.Value)
                    )
                    .ThenByReleaseType()
                : queryable
                    .OrderByDescending(result =>
                        DateOnly.FromDateTime(result.Value.LatestPublishedReleaseVersion!.Published!.Value)
                    )
                    .ThenByReleaseType(),
            Relevance => sortDirection == Asc
                ? queryable.OrderBy(result => result.Rank)
                : queryable.OrderByDescending(result => result.Rank),
            Title => sortDirection == Asc
                ? queryable.OrderBy(result => result.Value.Title)
                : queryable.OrderByDescending(result => result.Value.Title),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, message: null),
        };

        // Then sort by publication id to provide a stable sort order
        orderedQueryable = orderedQueryable.ThenBy(result => result.Value.Id);

        // Get the total results count for the paginated response
        var totalResults = await orderedQueryable.CountAsync();

        // Apply offset pagination and execute the query
        var results = await orderedQueryable
            .Paginate(page, pageSize)
            .Select(result => new PublicationSearchResultViewModel
            {
                Id = result.Value.Id,
                Slug = result.Value.Slug,
                LatestReleaseSlug = result.Value.LatestPublishedReleaseVersion!.Release.Slug,
                Summary = result.Value.Summary,
                Title = result.Value.Title,
                Theme = result.Value.Theme.Title,
                Published = result.Value.LatestPublishedReleaseVersion!.Published!.Value,
                Type = result.Value.LatestPublishedReleaseVersion!.Type,
                Rank = result.Rank,
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return new PaginatedListViewModel<PublicationSearchResultViewModel>(results, totalResults, page, pageSize);
    }
}
