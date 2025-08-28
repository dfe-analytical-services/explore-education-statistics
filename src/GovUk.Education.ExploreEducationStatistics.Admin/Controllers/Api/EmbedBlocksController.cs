using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class EmbedBlocksController : ControllerBase
{
    private readonly IEmbedBlockService _embedBlockService;

    public EmbedBlocksController(IEmbedBlockService embedBlockService)
    {
        _embedBlockService = embedBlockService;
    }

    [HttpPost("release/{releaseVersionId:guid}/embed-blocks")]
    public async Task<ActionResult<EmbedBlockLinkViewModel>> CreateEmbedBlockBlock(
        Guid releaseVersionId,
        EmbedBlockCreateRequest request)
    {
        return await _embedBlockService
            .Create(releaseVersionId, request)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/embed-blocks/{contentBlockId:guid}")]
    public async Task<ActionResult<EmbedBlockLinkViewModel>> UpdateEmbedBlock(
        Guid releaseVersionId,
        Guid contentBlockId,
        EmbedBlockUpdateRequest request)
    {
        return await _embedBlockService
            .Update(releaseVersionId: releaseVersionId,
                contentBlockId: contentBlockId,
                request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/embed-blocks/{contentBlockId:guid}")]
    public async Task<ActionResult<Unit>> DeleteEmbedBlock(
        Guid releaseVersionId,
        Guid contentBlockId)
    {
        return await _embedBlockService
            .Delete(releaseVersionId: releaseVersionId,
                contentBlockId: contentBlockId)
            .HandleFailuresOrOk();
    }
}
