using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class FeaturedTableController : ControllerBase
{
    private readonly IFeaturedTableService _featuredTableService;

    public FeaturedTableController(IFeaturedTableService featuredTableService)
    {
        _featuredTableService = featuredTableService;
    }

    [HttpGet("releases/{releaseId:guid}/featured-tables/{dataBlockId:guid}")]
    public async Task<ActionResult<FeaturedTableViewModel>> Get(Guid releaseId, Guid dataBlockId)
    {
        return await _featuredTableService
            .Get(
                releaseId: releaseId,
                dataBlockId: dataBlockId)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseId:guid}/featured-tables")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> List(Guid releaseId)
    {
        return await _featuredTableService
            .List(releaseId)
            .HandleFailuresOrOk();
    }

    [HttpPost("releases/{releaseId:guid}/featured-tables")]
    public async Task<ActionResult<FeaturedTableViewModel>> Create(
        Guid releaseId,
        FeaturedTableCreateRequest request)
    {
        return await _featuredTableService
            .Create(releaseId, request)
            .HandleFailuresOrOk();
    }

    [HttpPost("releases/{releaseId:guid}/featured-tables/{dataBlockId:guid}")]
    public async Task<ActionResult<FeaturedTableViewModel>> Update(
        Guid releaseId,
        Guid dataBlockId,
        FeaturedTableUpdateRequest request)
    {
        return await _featuredTableService
            .Update(
                releaseId: releaseId,
                dataBlockId: dataBlockId,
                request: request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("releases/{releaseId:guid}/featured-tables/{dataBlockId:guid}")]
    public async Task<ActionResult> Delete(Guid releaseId, Guid dataBlockId)
    {
        return await _featuredTableService
            .Delete(releaseId: releaseId, dataBlockId: dataBlockId)
            .HandleFailuresOrNoContent();
    }

    [HttpPut("releases/{releaseId:guid}/featured-tables/order")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> Reorder(
        Guid releaseId,
        List<Guid> newOrder)
    {
        return await _featuredTableService
            .Reorder(releaseId, newOrder)
            .HandleFailuresOrOk();
    }
}
