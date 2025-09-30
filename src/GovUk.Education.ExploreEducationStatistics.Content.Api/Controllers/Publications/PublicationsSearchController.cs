#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;

/// <summary>
/// Handles searching for publications.
/// To be removed by EES-6063 once the new Find Statistics search is fully adopted,
/// and the Public API no longer relies on it (see EES-6470).
/// </summary>
[Route("api/publications")]
[ApiController]
public class PublicationsSearchController(IPublicationsSearchService publicationsSearchService)
    : ControllerBase
{
    [MemoryCache(
        typeof(ListPublicationsGetCacheKey),
        durationInSeconds: 10,
        expiryScheduleCron: HalfHourlyExpirySchedule
    )]
    public async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> GetPublications(
        [FromQuery] PublicationsListGetRequest request,
        CancellationToken cancellationToken = default
    ) =>
        await publicationsSearchService.GetPublications(
            request.ReleaseType,
            request.ThemeId,
            request.Search,
            request.Sort,
            request.SortDirection,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken
        );

    [MemoryCache(
        typeof(ListPublicationsPostCacheKey),
        durationInSeconds: 10,
        expiryScheduleCron: HalfHourlyExpirySchedule
    )]
    [HttpPost]
    public async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> GetPublications(
        [FromBody] PublicationsListPostRequest request,
        CancellationToken cancellationToken = default
    ) =>
        await publicationsSearchService.GetPublications(
            request.ReleaseType,
            request.ThemeId,
            request.Search,
            request.Sort,
            request.SortDirection,
            page: request.Page,
            pageSize: request.PageSize,
            publicationIds: request.PublicationIds,
            cancellationToken: cancellationToken
        );
}
