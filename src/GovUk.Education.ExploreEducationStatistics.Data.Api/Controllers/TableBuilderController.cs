using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public ActionResult<TableBuilderResultViewModel> Query([FromBody] ObservationQueryContext query)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var tableBuilderResultViewModel = _dataService.Query(query);

            stopwatch.Stop();
            _logger.LogDebug("Query {Query} executed in {Time} ms", query, stopwatch.Elapsed.TotalMilliseconds);

            return tableBuilderResultViewModel;
        }
    }
}