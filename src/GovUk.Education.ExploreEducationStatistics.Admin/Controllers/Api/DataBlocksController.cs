using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class DataBlocksController : ControllerBase
    {
        private readonly IDataBlockService _dataBlockService;

        public DataBlocksController(IDataBlockService dataBlockService)
        {
            _dataBlockService = dataBlockService;
        }

        [HttpPost("release/{releaseId}/datablocks")]
        public async Task<ActionResult<DataBlockViewModel>> CreateDataBlock(
            Guid releaseId,
            CreateDataBlockViewModel dataBlock)
        {
            return await _dataBlockService.Create(releaseId, dataBlock)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/datablocks/{id}")]
        public async Task<ActionResult> DeleteDataBlock(Guid releaseId, Guid id)
        {
            return await _dataBlockService.Delete(releaseId, id)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("release/{releaseId}/datablocks/{id}/delete-plan")]
        public async Task<ActionResult<DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid id)
        {
            return await _dataBlockService.GetDeletePlan(releaseId, id)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/datablocks")]
        public async Task<ActionResult<List<DataBlockViewModel>>> GetDataBlocks(Guid releaseId)
        {
            return await _dataBlockService.List(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> GetDataBlock(Guid id)
        {
            return await _dataBlockService.Get(id)
                .HandleFailuresOrOk();
        }

        [HttpPut("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlock(Guid id,
            UpdateDataBlockViewModel dataBlock)
        {
            return await _dataBlockService.Update(id, dataBlock)
                .HandleFailuresOrOk();
        }
    }
}