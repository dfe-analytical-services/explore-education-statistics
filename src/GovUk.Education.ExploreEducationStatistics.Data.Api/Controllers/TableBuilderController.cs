using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IUserService _userService;

        public TableBuilderController(
            ITableBuilderService tableBuilderService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService)
        {
            _tableBuilderService = tableBuilderService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
        }

        [HttpPost]
        public Task<ActionResult<TableBuilderResultViewModel>> Query([FromBody] ObservationQueryContext query)
        {
            return _tableBuilderService.Query(query).HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}")]
        public Task<ActionResult<TableBuilderResultViewModel>> Query(
            Guid releaseId,
            [FromBody] ObservationQueryContext query)
        {
            return _tableBuilderService.Query(releaseId, query).HandleFailuresOrOk();
        }

        [ResponseCache(Duration = 300)]
        [HttpGet("release/{releaseId}/datablock/{dataBlockId}")]
        public async Task<ActionResult<TableBuilderResultViewModel>> QueryForDataBlock(
            Guid releaseId,
            Guid dataBlockId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<ReleaseContentBlock>(
                    query => query
                        .Include(rcb => rcb.ContentBlock)
                        .Include(rcb => rcb.Release)
                        .Where(
                            rcb => rcb.ReleaseId == releaseId
                                   && rcb.ContentBlockId == dataBlockId
                        )
                )
                .OnSuccessDo(block => this.CacheWithLastModified(block.Release.Published))
                .OnSuccess(
                    async block =>
                    {
                        if (block.ContentBlock is DataBlock dataBlock)
                        {
                            return await _userService.CheckCanViewRelease(block.Release)
                                .OnSuccess(_ => _tableBuilderService.Query(block.ReleaseId, dataBlock.Query));
                        }

                        return new NotFoundResult();
                    }
                )
                .HandleFailuresOrOk();
        }
    }
}