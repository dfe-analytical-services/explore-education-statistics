using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
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

        [HttpPost("release/{releaseId:guid}/embed-blocks")]
        public async Task<ActionResult<EmbedBlockLinkViewModel>> CreateEmbedBlockBlock(
            Guid releaseId,
            EmbedBlockCreateRequest request)
        {
            return await _embedBlockService
                .Create(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId:guid}/embed-blocks/{contentBlockId:guid}")]
        public async Task<ActionResult<EmbedBlockLinkViewModel>> UpdateEmbedBlock(
            Guid releaseId,
            Guid contentBlockId,
            EmbedBlockUpdateRequest request)
        {
            return await _embedBlockService
                .Update(releaseId, contentBlockId, request)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId:guid}/embed-blocks/{contentBlockId:guid}")]
        public async Task<ActionResult<Unit>> DeleteEmbedBlock(
            Guid releaseId,
            Guid contentBlockId)
        {
            return await _embedBlockService
                .Delete(releaseId, contentBlockId)
                .HandleFailuresOrOk();
        }
    }
}
