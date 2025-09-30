#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class BoundaryLevelController(IBoundaryLevelService boundaryLevelService) : ControllerBase
{
    [HttpGet("boundary-level")]
    public async Task<ActionResult<List<BoundaryLevelViewModel>>> ListBoundaryLevels()
    {
        return await boundaryLevelService
            .ListBoundaryLevels()
            .OnSuccess(boundaryLevels => boundaryLevels.Select(BuildViewModel).ToList())
            .HandleFailuresOrOk();
    }

    [HttpGet("boundary-level/{id:long}")]
    public async Task<ActionResult<BoundaryLevelViewModel>> GetBoundaryLevel(long id)
    {
        return await boundaryLevelService
            .GetBoundaryLevel(id)
            .OnSuccess(BuildViewModel)
            .HandleFailuresOrOk();
    }

    [HttpPatch("boundary-level")]
    public async Task<ActionResult> UpdateBoundaryLevel(BoundaryLevelUpdateRequest updateRequest)
    {
        return await boundaryLevelService
            .UpdateBoundaryLevel(updateRequest.Id, updateRequest.Label)
            .HandleFailuresOrNoContent();
    }

    [HttpPost("boundary-level")]
    public async Task<ActionResult<Unit>> CreateBoundaryLevel(
        [FromForm] BoundaryLevelCreateRequest request
    )
    {
        return await boundaryLevelService
            .CreateBoundaryLevel(request.Level, request.Label, request.Published, request.File)
            .HandleFailuresOrNoContent();
    }

    private static BoundaryLevelViewModel BuildViewModel(BoundaryLevel boundaryLevel)
    {
        return new BoundaryLevelViewModel
        {
            Id = boundaryLevel.Id,
            Label = boundaryLevel.Label!,
            Level = boundaryLevel.Level,
            Published = boundaryLevel.Published,
        };
    }
}
