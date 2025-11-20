#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RelatedInformation;

[Route("api")]
[ApiController]
[Authorize]
public class RelatedInformationController(IRelatedInformationService relatedInformationService) : ControllerBase
{
    [HttpPost("release/{releaseVersionId:guid}/content/related-information")]
    public Task<ActionResult<List<RelatedInformationDto>>> CreateRelatedInformation(
        Guid releaseVersionId,
        RelatedInformationCreateRequest request,
        CancellationToken cancellationToken = default
    ) =>
        relatedInformationService
            .CreateRelatedInformation(
                releaseVersionId: releaseVersionId,
                title: request.Title,
                url: request.Url,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));

    [HttpGet("release/{releaseVersionId:guid}/content/related-information")]
    public Task<ActionResult<List<RelatedInformationDto>>> GetRelatedInformation(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    ) => relatedInformationService.GetRelatedInformation(releaseVersionId, cancellationToken).HandleFailuresOrOk();

    [HttpPut("release/{releaseVersionId:guid}/content/related-information")]
    public Task<ActionResult<List<RelatedInformationDto>>> UpdateRelatedInformation(
        Guid releaseVersionId,
        List<RelatedInformationUpdateRequest> request,
        CancellationToken cancellationToken
    ) =>
        relatedInformationService
            .UpdateRelatedInformation(
                releaseVersionId: releaseVersionId,
                title: request.Title,
                url: request.Url,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();

    [HttpDelete("release/{releaseVersionId:guid}/content/related-information/{relatedInformationId:guid}")]
    public Task<ActionResult<List<RelatedInformationDto>>> DeleteRelatedInformation(
        Guid releaseVersionId,
        Guid relatedInformationId,
        CancellationToken cancellationToken = default
    ) =>
        relatedInformationService
            .DeleteRelatedInformation(
                releaseVersionId: releaseVersionId,
                relatedInformationId: relatedInformationId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
}
