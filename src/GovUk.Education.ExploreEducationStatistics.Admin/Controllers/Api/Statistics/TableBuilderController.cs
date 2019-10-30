using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Web.Http;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class TableBuilderController : ControllerBase
    {
        private readonly IDataService<TableBuilderResultViewModel> _dataService;
        private readonly ILogger _logger;

        public TableBuilderController(IDataService<TableBuilderResultViewModel> dataService,
            ILogger<TableBuilderController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<TableBuilderResultViewModel> Query([FromUri, Required] ReleaseId releaseId,
            [FromBody] ObservationQueryContext query)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var tableBuilderResultViewModel = _dataService.Query(query, releaseId);

            stopwatch.Stop();
            _logger.LogDebug("Query {Query} executed in {Time} ms", query, stopwatch.Elapsed.TotalMilliseconds);

            return tableBuilderResultViewModel;
        }
    }
}