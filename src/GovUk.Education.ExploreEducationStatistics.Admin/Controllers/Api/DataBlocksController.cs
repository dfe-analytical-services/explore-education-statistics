using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataBlockId = System.Guid;
using ContentSectionId = System.Guid;
using PublicationId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class DataBlocksController : ControllerBase
    {
        private readonly IDataBlockService _dataBlockService;
        private readonly IReleaseService _releaseService;

        public DataBlocksController(IDataBlockService dataBlockService, IReleaseService releaseService)
        {
            _dataBlockService = dataBlockService;
            _releaseService = releaseService;
        }

        [HttpPost("release/{releaseId}/datablocks")]
        public async Task<ActionResult<DataBlockViewModel>> CreateDataBlockAsync(ReleaseId releaseId,
            CreateDataBlockViewModel dataBlock)
        {
            return await CheckReleaseExistsAsync(releaseId, async () =>
                Ok(await _dataBlockService.CreateAsync(releaseId, dataBlock)));
        }

        [HttpGet("release/{releaseId}/datablocks")]
        public async Task<ActionResult<List<DataBlockViewModel>>> GetDataBlocksAsync(ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => Ok(await _dataBlockService.ListAsync(releaseId)));
        }

        [HttpGet("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> GetDataBlockAsync(DataBlockId id)
        {
            return await _dataBlockService.GetAsync(id);
        }
        
        [HttpPut("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlockAsync(DataBlockId id, UpdateDataBlockViewModel dataBlock)
        {
            return await _dataBlockService.UpdateAsync(id, dataBlock);
        }

        private async Task<ActionResult> CheckReleaseExistsAsync(ReleaseId releaseId, Func<Task<ActionResult>> andThen)
        {
            var release = await _releaseService.GetAsync(releaseId);
            if (release == null)
            {
                return NotFound();
            }

            return await andThen.Invoke();
        }
    }
}