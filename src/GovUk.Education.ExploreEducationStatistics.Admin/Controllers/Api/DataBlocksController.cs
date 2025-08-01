using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

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

    [HttpPost("releases/{releaseVersionId:guid}/data-blocks")]
    public async Task<ActionResult<DataBlockViewModel>> CreateDataBlock(
        Guid releaseVersionId,
        DataBlockCreateRequest dataBlock)
    {
        return await _dataBlockService
            .Create(releaseVersionId, dataBlock)
            .HandleFailuresOrOk();
    }

    [HttpDelete("releases/{releaseVersionId:guid}/data-blocks/{dataBlockVersionId:guid}")]
    public async Task<ActionResult> DeleteDataBlock(Guid releaseVersionId,
        Guid dataBlockVersionId)
    {
        return await _dataBlockService
            .Delete(releaseVersionId: releaseVersionId,
                dataBlockVersionId: dataBlockVersionId)
            .HandleFailuresOrNoContent();
    }

    [HttpGet("releases/{releaseVersionId:guid}/data-blocks/{dataBlockVersionId:guid}/delete-plan")]
    public async Task<ActionResult<DeleteDataBlockPlanViewModel>> GetDeletePlan(Guid releaseVersionId,
        Guid dataBlockVersionId)
    {
        return await _dataBlockService
            .GetDeletePlan(releaseVersionId: releaseVersionId,
                dataBlockVersionId: dataBlockVersionId)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/data-blocks")]
    public async Task<ActionResult<List<DataBlockSummaryViewModel>>> List(Guid releaseVersionId)
    {
        return await _dataBlockService
            .List(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpGet("data-blocks/{dataBlockVersionId:guid}")]
    public async Task<ActionResult<DataBlockViewModel>> GetDataBlock(Guid dataBlockVersionId)
    {
        return await _dataBlockService
            .Get(dataBlockVersionId)
            .HandleFailuresOrOk();
    }

    [HttpPut("data-blocks/{dataBlockVersionId:guid}")]
    public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlock(Guid dataBlockVersionId,
        DataBlockUpdateRequest dataBlock)
    {
        return await _dataBlockService
            .Update(dataBlockVersionId, dataBlock)
            .HandleFailuresOrOk();
    }

    [HttpGet("release/{releaseVersionId:guid}/data-blocks/unattached")]
    public async Task<ActionResult<List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseVersionId)
    {
        return await _dataBlockService
            .GetUnattachedDataBlocks(releaseVersionId)
            .HandleFailuresOrOk();
    }
}
