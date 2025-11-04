#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class TableBuilderMetaController : ControllerBase
{
    private readonly ISubjectMetaService _subjectMetaService;

    public TableBuilderMetaController(ISubjectMetaService subjectMetaService)
    {
        _subjectMetaService = subjectMetaService;
    }

    [HttpGet("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}")]
    [BlobCache(typeof(PrivateSubjectMetaCacheKey))]
    public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid releaseVersionId, Guid subjectId)
    {
        return _subjectMetaService
            .GetSubjectMeta(releaseVersionId: releaseVersionId, subjectId: subjectId)
            .HandleFailuresOrOk();
    }

    [HttpPost("data/release/{releaseVersionId:guid}/meta/subject")]
    public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
        Guid releaseVersionId,
        [FromBody] LocationsOrTimePeriodsQueryRequest request,
        CancellationToken cancellationToken
    )
    {
        return _subjectMetaService.FilterSubjectMeta(releaseVersionId, request, cancellationToken).HandleFailuresOrOk();
    }

    [HttpPatch("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}/filters")]
    public Task<ActionResult<Unit>> UpdateFilters(
        Guid releaseVersionId,
        Guid subjectId,
        List<FilterUpdateViewModel> request
    )
    {
        return _subjectMetaService
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
        return _subjectMetaService
            .UpdateSubjectIndicators(releaseVersionId: releaseVersionId, subjectId: subjectId, request)
            .HandleFailuresOrOk();
    }
}
