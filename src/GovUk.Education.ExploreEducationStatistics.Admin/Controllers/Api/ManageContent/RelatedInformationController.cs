using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;

[Route("api")]
[ApiController]
[Authorize]
public class RelatedInformationController(IRelatedInformationService relatedInformationService) : ControllerBase
{
    [HttpGet("release/{releaseVersionId:guid}/content/related-information")]
    public async Task<ActionResult<List<Link>>> GetRelatedInformation(Guid releaseVersionId)
    {
        return await relatedInformationService
            .GetRelatedInformationAsync(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/content/related-information")]
    public async Task<ActionResult<List<Link>>> AddRelatedInformation(
        CreateUpdateLinkRequest request,
        Guid releaseVersionId)
    {
        return await relatedInformationService
            .AddRelatedInformationAsync(releaseVersionId, request)
            .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));
    }

    [HttpPut("release/{releaseVersionId:guid}/content/related-information")]
    public async Task<ActionResult<List<Link>>> UpdateRelatedInformation(
        List<CreateUpdateLinkRequest> updatedLinkRequests,
        Guid releaseVersionId,
        CancellationToken cancellationToken)
    {
        return await relatedInformationService
            .UpdateRelatedInformation(
                releaseVersionId,
                updatedLinkRequests,
                cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/content/related-information/{relatedInformationId:guid}")]
    public async Task<ActionResult<List<Link>>> DeleteRelatedInformation(
        Guid releaseVersionId,
        Guid relatedInformationId)
    {
        return await relatedInformationService
            .DeleteRelatedInformationAsync(releaseVersionId: releaseVersionId,
                relatedInformationId: relatedInformationId)
            .HandleFailuresOrOk();
    }
}
