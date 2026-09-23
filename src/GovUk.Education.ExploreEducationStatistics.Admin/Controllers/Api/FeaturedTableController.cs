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

    [HttpGet("releases/{releaseVersionId:guid}/featured-tables/{dataBlockVersionId:guid}")]
    public async Task<ActionResult<FeaturedTableViewModel>> Get(Guid releaseVersionId, Guid dataBlockVersionId)
    {
        return await _featuredTableService
            .Get(releaseVersionId: releaseVersionId, dataBlockVersionId: dataBlockVersionId)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/featured-tables")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> List(Guid releaseVersionId)
    {
        return await _featuredTableService.List(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpPost("releases/{releaseVersionId:guid}/featured-tables")]
    public async Task<ActionResult<FeaturedTableViewModel>> Create(
        Guid releaseVersionId,
        FeaturedTableCreateRequest request
    )
    {
        return await _featuredTableService.Create(releaseVersionId, request).HandleFailuresOrOk();
    }

    [HttpPost("releases/{releaseVersionId:guid}/featured-tables/{dataBlockVersionId:guid}")]
    public async Task<ActionResult<FeaturedTableViewModel>> Update(
        Guid releaseVersionId,
        Guid dataBlockVersionId,
        FeaturedTableUpdateRequest request
    )
    {
        return await _featuredTableService
            .Update(releaseVersionId: releaseVersionId, dataBlockVersionId: dataBlockVersionId, request: request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("releases/{releaseVersionId:guid}/featured-tables/{dataBlockVersionId:guid}")]
    public async Task<ActionResult> Delete(Guid releaseVersionId, Guid dataBlockVersionId)
    {
        return await _featuredTableService
            .Delete(releaseVersionId: releaseVersionId, dataBlockVersionId: dataBlockVersionId)
            .HandleFailuresOrNoContent();
    }

    [HttpPut("releases/{releaseVersionId:guid}/featured-tables/order")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> Reorder(Guid releaseVersionId, List<Guid> newOrder)
    {
        return await _featuredTableService.Reorder(releaseVersionId, newOrder).HandleFailuresOrOk();
    }
}
