#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/preview-tokens")]
public class PreviewTokenController(IPreviewTokenService previewTokenService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PreviewTokenViewModel>> CreatePreviewToken(
        [FromBody] PreviewTokenCreateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await previewTokenService
            .CreatePreviewToken(request.DataSetVersionId, 
                request.Label, 
                request.Activates, 
                request.Expires, 
                cancellationToken)
            .HandleFailuresOr(previewToken =>
                CreatedAtAction(nameof(GetPreviewToken), new { previewTokenId = previewToken.Id }, previewToken)
            );
    }

    [HttpGet("{previewTokenId:guid}")]
    public async Task<ActionResult<PreviewTokenViewModel>> GetPreviewToken(
        Guid previewTokenId,
        CancellationToken cancellationToken
    )
    {
        return await previewTokenService.GetPreviewToken(previewTokenId, cancellationToken).HandleFailuresOrOk();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PreviewTokenViewModel>>> ListPreviewTokens(
        [FromQuery] Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        return await previewTokenService.ListPreviewTokens(dataSetVersionId, cancellationToken).HandleFailuresOrOk();
    }

    [HttpPost("{previewTokenId:guid}/revoke")]
    public async Task<ActionResult<PreviewTokenViewModel>> RevokePreviewToken(
        Guid previewTokenId,
        CancellationToken cancellationToken
    )
    {
        return await previewTokenService.RevokePreviewToken(previewTokenId, cancellationToken).HandleFailuresOrOk();
    }
}
