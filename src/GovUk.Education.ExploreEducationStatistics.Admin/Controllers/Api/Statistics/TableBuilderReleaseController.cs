#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces.IReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class TableBuilderReleaseController : ControllerBase
{
    private readonly IReleaseService _releaseService;

    public TableBuilderReleaseController(IReleaseService releaseService)
    {
        _releaseService = releaseService;
    }

    [HttpGet("data/releases/{releaseVersionId:guid}/subjects")]
    public async Task<ActionResult<List<SubjectViewModel>>> ListSubjects(Guid releaseVersionId)
    {
        return await _releaseService
            .ListSubjects(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpGet("data/releases/{releaseVersionId:guid}/featured-tables")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseVersionId)
    {
        return await _releaseService
            .ListFeaturedTables(releaseVersionId)
            .HandleFailuresOrOk();
    }
}
