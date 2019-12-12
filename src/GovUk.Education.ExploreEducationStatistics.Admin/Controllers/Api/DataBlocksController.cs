using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class DataBlocksController : ControllerBase
    {
        private readonly IDataBlockService _dataBlockService;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper;
        private readonly PersistenceHelper<DataBlock, Guid> _dataBlockHelper;

        public DataBlocksController(ContentDbContext context, IDataBlockService dataBlockService)
        {
            _dataBlockService = dataBlockService;
            _releaseHelper = new PersistenceHelper<Release, Guid>(context, context.Releases, ReleaseNotFound);
            _dataBlockHelper = new PersistenceHelper<DataBlock, Guid>(context, context.DataBlocks, ContentBlockNotFound);
        }

        [HttpPost("release/{releaseId}/datablocks")]
        public Task<ActionResult<DataBlockViewModel>> CreateDataBlockAsync(Guid releaseId,
            CreateDataBlockViewModel dataBlock)
        {
            return this.HandlingValidationErrorsAsync(() => 
                _releaseHelper.CheckEntityExists(releaseId, release =>
                    _dataBlockService.CreateAsync(releaseId, dataBlock))
                , Ok);
        }

        [HttpDelete("datablocks/{id}")]
        public Task<ActionResult> DeleteDataBlockAsync(Guid id)
        {
            return this.HandlingValidationErrorsAsyncNoReturn(() => 
                _dataBlockHelper.CheckEntityExists(id, _ => _dataBlockService.DeleteAsync(id)),
                () => new NoContentResult());
        }

        [HttpGet("release/{releaseId}/datablocks")]
        public Task<ActionResult<List<DataBlockViewModel>>> GetDataBlocksAsync(Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(() => 
                _releaseHelper.CheckEntityExists(releaseId, release =>
                    _dataBlockService.ListAsync(releaseId))
                , Ok);
        }

        [HttpGet("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> GetDataBlockAsync(Guid id)
        {
            return Ok(await _dataBlockService.GetAsync(id));
        }

        [HttpPut("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlockAsync(Guid id,
            UpdateDataBlockViewModel dataBlock)
        {
            return Ok(await _dataBlockService.UpdateAsync(id, dataBlock));
        }
    }
}