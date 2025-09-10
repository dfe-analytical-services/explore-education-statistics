#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;

[Route("api")]
[ApiController]
public class ReleaseController : ControllerBase
{
    private readonly IReleaseService _releaseService;
    private readonly ICacheKeyService _cacheKeyService;

    public ReleaseController(
        IReleaseService releaseService,
        ICacheKeyService cacheKeyService)
    {
        _releaseService = releaseService;
        _cacheKeyService = cacheKeyService;
    }

    [HttpGet("releases/{releaseVersionId:guid}/subjects")]
    public async Task<ActionResult<List<SubjectViewModel>>> ListSubjects(Guid releaseVersionId)
    {
        return await _cacheKeyService
            .CreateCacheKeyForReleaseSubjects(releaseVersionId)
            .OnSuccess(ListSubjects)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/featured-tables")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseVersionId)
    {
        return await _releaseService
            .ListFeaturedTables(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [BlobCache(typeof(ReleaseSubjectsCacheKey))]
    private Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(ReleaseSubjectsCacheKey cacheKey)
    {
        return _releaseService.ListSubjects(cacheKey.ReleaseVersionId);
    }
}
