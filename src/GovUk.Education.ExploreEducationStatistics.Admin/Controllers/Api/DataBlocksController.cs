using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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

        [HttpPost("releases/{releaseId:guid}/data-blocks")]
        public async Task<ActionResult<DataBlockViewModel>> CreateDataBlock(
            Guid releaseId,
            DataBlockCreateViewModel dataBlock)
        {
            return await _dataBlockService
                .Create(releaseId, dataBlock)
                .HandleFailuresOrOk();
        }

        [HttpDelete("releases/{releaseId:guid}/data-blocks/{id:guid}")]
        public async Task<ActionResult> DeleteDataBlock(Guid releaseId, Guid id)
        {
            return await _dataBlockService
                .Delete(releaseId, id)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("releases/{releaseId:guid}/data-blocks/{id:guid}/delete-plan")]
        public async Task<ActionResult<DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid id)
        {
            return await _dataBlockService
                .GetDeletePlan(releaseId, id)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/data-blocks")]
        public async Task<ActionResult<List<DataBlockSummaryViewModel>>> List(Guid releaseId)
        {
            return await _dataBlockService
                .List(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("data-blocks/{id:guid}")]
        public async Task<ActionResult<DataBlockViewModel>> GetDataBlock(Guid id)
        {
            return await _dataBlockService
                .Get(id)
                .HandleFailuresOrOk();
        }

        [HttpPut("data-blocks/{id:guid}")]
        public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlock(Guid id,
            DataBlockUpdateViewModel dataBlock)
        {
            return await _dataBlockService
                .Update(id, dataBlock)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/data-blocks/unattached")]
        public async Task<ActionResult<List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseId)
        {
            return await _dataBlockService
                .GetUnattachedDataBlocks(releaseId)
                .HandleFailuresOrOk();
        }

    }
}
