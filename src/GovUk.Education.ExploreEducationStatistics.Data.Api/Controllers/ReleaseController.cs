#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;

[Route("api")]
[ApiController]
public class ReleaseController(
    IReleaseService releaseService,
    ICacheKeyService cacheKeyService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<ReleaseController> logger
) : ControllerBase
{
    [HttpGet("releases/{releaseVersionId:guid}/subjects")]
    public async Task<ActionResult<List<SubjectViewModel>>> ListSubjects(Guid releaseVersionId)
    {
        return await cacheKeyService
            .CreateCacheKeyForReleaseSubjects(releaseVersionId)
            .OnSuccess(ListSubjects)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/featured-tables")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseVersionId)
    {
        return await releaseService.ListFeaturedTables(releaseVersionId).HandleFailuresOrOk();
    }

    private Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(ReleaseSubjectsCacheKey cacheKey)
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: cacheKey,
            createIfNotExistsFn: () => releaseService.ListSubjects(cacheKey.ReleaseVersionId),
            logger: logger
        );
    }
}
