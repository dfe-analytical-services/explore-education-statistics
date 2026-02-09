#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class TableBuilderMetaController(
    ISubjectMetaService subjectMetaService,
    IPrivateBlobCacheService privateBlobCacheService,
    ILogger<TableBuilderMetaController> logger
) : ControllerBase
{
    [HttpGet("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}")]
    public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid releaseVersionId, Guid subjectId)
    {
        return privateBlobCacheService.GetOrCreateAsync(
            cacheKey: new PrivateSubjectMetaCacheKey(releaseVersionId: releaseVersionId, subjectId: subjectId),
            createIfNotExistsFn: () =>
                subjectMetaService
                    .GetSubjectMeta(releaseVersionId: releaseVersionId, subjectId: subjectId)
                    .HandleFailuresOrOk(),
            logger: logger
        );
    }

    [HttpPost("data/release/{releaseVersionId:guid}/meta/subject")]
    public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
        Guid releaseVersionId,
        [FromBody] LocationsOrTimePeriodsQueryRequest request,
        CancellationToken cancellationToken
    )
    {
        return subjectMetaService.FilterSubjectMeta(releaseVersionId, request, cancellationToken).HandleFailuresOrOk();
    }

    [HttpPatch("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}/filters")]
    public Task<ActionResult<Unit>> UpdateFilters(
        Guid releaseVersionId,
        Guid subjectId,
        List<FilterUpdateViewModel> request
    )
    {
        return subjectMetaService
            .UpdateSubjectFilters(releaseVersionId: releaseVersionId, subjectId: subjectId, request)
            .HandleFailuresOrOk();
    }

    [HttpPatch("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}/indicators")]
    public Task<ActionResult<Unit>> UpdateIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        List<IndicatorGroupUpdateViewModel> request
    )
    {
        return subjectMetaService
            .UpdateSubjectIndicators(releaseVersionId: releaseVersionId, subjectId: subjectId, request)
            .HandleFailuresOrOk();
    }
}
