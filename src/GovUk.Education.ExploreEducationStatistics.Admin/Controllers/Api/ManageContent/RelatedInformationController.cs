using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;

[Route("api")]
[ApiController]
[Authorize]
public class RelatedInformationController : ControllerBase
{
    private readonly IRelatedInformationService _relatedInformationService;

    public RelatedInformationController(IRelatedInformationService relatedInformationService)
    {
        _relatedInformationService = relatedInformationService;
    }

    [HttpGet("release/{releaseVersionId:guid}/content/related-information")]
    public async Task<ActionResult<List<Link>>> GetRelatedInformation(Guid releaseVersionId)
    {
        return await _relatedInformationService
            .GetRelatedInformationAsync(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/content/related-information")]
    public async Task<ActionResult<List<Link>>> AddRelatedInformation(CreateUpdateLinkRequest request,
        Guid releaseVersionId)
    {
        return await _relatedInformationService
            .AddRelatedInformationAsync(releaseVersionId, request)
            .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));
    }

    [HttpPut("release/{releaseVersionId:guid}/content/related-information/{relatedInformationId:guid}")]
    public async Task<ActionResult<List<Link>>> UpdateRelatedInformation(
        CreateUpdateLinkRequest request, Guid releaseVersionId, Guid relatedInformationId)
    {
        return await _relatedInformationService
            .UpdateRelatedInformationAsync(releaseVersionId: releaseVersionId,
                relatedInformationId: relatedInformationId,
                request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/content/related-information/{relatedInformationId:guid}")]
    public async Task<ActionResult<List<Link>>> DeleteRelatedInformation(
        Guid releaseVersionId, Guid relatedInformationId)
    {
        return await _relatedInformationService
            .DeleteRelatedInformationAsync(releaseVersionId: releaseVersionId,
                relatedInformationId: relatedInformationId)
            .HandleFailuresOrOk();
    }
}
