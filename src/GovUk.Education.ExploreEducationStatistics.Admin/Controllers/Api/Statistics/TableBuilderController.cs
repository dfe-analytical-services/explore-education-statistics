using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public TableBuilderController(
            ITableBuilderService tableBuilderService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService,
            ILogger<TableBuilderController> logger)
        {
            _tableBuilderService = tableBuilderService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("release/{releaseId}")]
        public Task<ActionResult<TableBuilderResultViewModel>> Query(Guid releaseId, [FromBody] ObservationQueryContext query)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var tableBuilderResultViewModel = _tableBuilderService.Query(releaseId, query);

            stopwatch.Stop();
            _logger.LogDebug("Query {Query} executed in {Time} ms", query, stopwatch.Elapsed.TotalMilliseconds);

            return tableBuilderResultViewModel.HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data-block/{dataBlockId}")]
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
                .OnSuccess(
                    async block =>
                    {
                        if (block.ContentBlock is DataBlock dataBlock)
                        {
                            var query = dataBlock.Query.Clone();
                            query.IncludeGeoJson = dataBlock.Charts.Any(chart => chart.Type == ChartType.Map);

                            return await _userService.CheckCanViewRelease(block.Release)
                                .OnSuccess(_ => _tableBuilderService.Query(block.ReleaseId, query));
                        }

                        return new NotFoundResult();
                    }
                )
                .HandleFailuresOrOk();
        }
    }
}