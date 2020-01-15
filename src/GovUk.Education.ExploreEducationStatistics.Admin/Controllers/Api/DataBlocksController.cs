using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        private readonly IPersistenceHelper<Release, Guid> _releaseHelper;
        private readonly IPersistenceHelper<DataBlock, Guid> _dataBlockHelper;

        public DataBlocksController(
            IDataBlockService dataBlockService, 
            IPersistenceHelper<Release, Guid> releaseHelper, 
            IPersistenceHelper<DataBlock, Guid> dataBlockHelper)
        {
            _dataBlockService = dataBlockService;
            _releaseHelper = releaseHelper;
            _dataBlockHelper = dataBlockHelper;
        }

        [HttpPost("release/{releaseId}/datablocks")]
        public async Task<ActionResult<DataBlockViewModel>> CreateDataBlockAsync(Guid releaseId,
            CreateDataBlockViewModel dataBlock)
        {
            // TODO EES-918 - remove in favour of checking for release inside service calls
            return await _releaseHelper
                .CheckEntityExists(releaseId)
                .OnSuccess(_ => _dataBlockService.CreateAsync(releaseId, dataBlock))
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("datablocks/{id}")]
        public Task<ActionResult> DeleteDataBlockAsync(Guid id)
        {
            // TODO EES-918 - remove in favour of checking for release inside service calls
            return _dataBlockHelper
                .CheckEntityExists(id)
                .OnSuccess(_ => _dataBlockService.DeleteAsync(id))
                .HandleFailuresOr(_ => new NoContentResult());
        }

        [HttpGet("release/{releaseId}/datablocks")]
        public async Task<ActionResult<List<DataBlockViewModel>>> GetDataBlocksAsync(Guid releaseId)
        {
            // TODO EES-918 - remove in favour of checking for release inside service calls
            return await _releaseHelper
                .CheckEntityExists(releaseId)
                .OnSuccess(_ => _dataBlockService.ListAsync(releaseId))
                .HandleFailuresOr(Ok);
        }

        [HttpGet("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> GetDataBlockAsync(Guid id)
        {
            return await _dataBlockHelper
                .CheckEntityExists(id)
                // TODO EES-918 - remove in favour of checking for release inside service calls
                .OnSuccess(_ => _dataBlockService.GetAsync(id))
                .HandleFailuresOr(Ok);
        }

        [HttpPut("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlockAsync(Guid id,
            UpdateDataBlockViewModel dataBlock)
        {
            return await _dataBlockHelper
                .CheckEntityExists(id)
                .OnSuccess(_ => _dataBlockService.UpdateAsync(id, dataBlock))
                .HandleFailuresOr(Ok);
        }
    }
}