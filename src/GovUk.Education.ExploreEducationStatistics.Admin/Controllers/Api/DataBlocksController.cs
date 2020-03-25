using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public DataBlocksController(
            IDataBlockService dataBlockService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _dataBlockService = dataBlockService;
            _persistenceHelper = persistenceHelper;
        }

        [HttpPost("release/{releaseId}/datablocks")]
        public async Task<ActionResult<DataBlockViewModel>> CreateDataBlockAsync(Guid releaseId,
            CreateDataBlockViewModel dataBlock)
        {
            // TODO EES-918 - remove in favour of checking for release inside service calls
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _dataBlockService.CreateAsync(releaseId, dataBlock))
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("datablocks/{id}")]
        public Task<ActionResult> DeleteDataBlockAsync(Guid id)
        {
            // TODO EES-918 - remove in favour of checking for release inside service calls
            return _persistenceHelper
                .CheckEntityExists<DataBlock>(id)
                .OnSuccess(_ => _dataBlockService.DeleteAsync(id))
                .HandleFailuresOr(_ => new NoContentResult());
        }

        [HttpGet("release/{releaseId}/datablocks")]
        public async Task<ActionResult<List<DataBlockViewModel>>> GetDataBlocksAsync(Guid releaseId)
        {
            // TODO EES-918 - remove in favour of checking for release inside service calls
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _dataBlockService.ListAsync(releaseId))
                .HandleFailuresOr(Ok);
        }

        [HttpGet("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> GetDataBlockAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlock>(id)
                // TODO EES-918 - remove in favour of checking for release inside service calls
                .OnSuccess(_ => _dataBlockService.GetAsync(id))
                .HandleFailuresOr(Ok);
        }

        [HttpPut("datablocks/{id}")]
        public async Task<ActionResult<DataBlockViewModel>> UpdateDataBlockAsync(Guid id,
            UpdateDataBlockViewModel dataBlock)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlock>(id)
                .OnSuccess(_ => _dataBlockService.UpdateAsync(id, dataBlock))
                .HandleFailuresOr(Ok);
        }
    }
}