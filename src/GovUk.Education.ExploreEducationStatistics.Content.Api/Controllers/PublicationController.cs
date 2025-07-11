#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[ApiController]
[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class PublicationController : ControllerBase
{
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IPublicationService _publicationService;

    public PublicationController(
        IPublicationCacheService publicationCacheService,
        IPublicationService publicationService)
    {
        _publicationCacheService = publicationCacheService;
        _publicationService = publicationService;
    }

    [HttpGet("publication-tree")]
    public async Task<ActionResult<IList<PublicationTreeThemeViewModel>>> GetPublicationTree(
        [FromQuery(Name = "publicationFilter")]
        PublicationTreeFilter? filter = null)
    {
        if (filter == null)
        {
            return new BadRequestResult();
        }

        return await _publicationCacheService
            .GetPublicationTree(filter.Value)
            .HandleFailuresOrOk();
    }

    [MemoryCache(typeof(ListPublicationsGetCacheKey), durationInSeconds: 10, expiryScheduleCron: HalfHourlyExpirySchedule)]
    [HttpGet("publications")]
    public async Task<ActionResult<PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        [FromQuery] PublicationsListGetRequest request)
    {
        return await _publicationService
            .ListPublications(
                request.ReleaseType,
                request.ThemeId,
                request.Search,
                request.Sort,
                request.SortDirection,
                page: request.Page,
                pageSize: request.PageSize)
            .HandleFailuresOrOk();
    }

    [MemoryCache(typeof(ListPublicationsPostCacheKey), durationInSeconds: 10, expiryScheduleCron: HalfHourlyExpirySchedule)]
    [HttpPost("publications")]
    public async Task<ActionResult<PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        [FromBody] PublicationsListPostRequest request)
    {
        return await _publicationService
            .ListPublications(
                request.ReleaseType,
                request.ThemeId,
                request.Search,
                request.Sort,
                request.SortDirection,
                page: request.Page,
                pageSize: request.PageSize,
                publicationIds: request.PublicationIds)
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationId:guid}/summary")]
    public async Task<ActionResult<PublishedPublicationSummaryViewModel>> GetPublicationSummary(Guid publicationId)
    {
        return await _publicationService
            .GetSummary(publicationId)
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/{slug}/title")]
    public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
    {
        return await _publicationCacheService.GetPublication(slug)
            .OnSuccess(p => new PublicationTitleViewModel
            {
                Id = p.Id,
                Title = p.Title,
            })
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/sitemap-items")]
    public async Task<ActionResult<List<PublicationSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken = default) =>
        await _publicationService.ListSitemapItems(cancellationToken)
            .HandleFailuresOrOk();
    
    [HttpGet("publicationInfos")]
    public async Task<ActionResult<IList<PublicationInfoViewModel>>> ListPublicationInfos(
        [FromQuery] GetPublicationInfosRequest request, 
        CancellationToken cancellationToken = default) =>
        Ok(await _publicationService.ListPublicationInfos(request.ThemeId, cancellationToken));
}
