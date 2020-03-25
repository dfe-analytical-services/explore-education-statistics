using System.Diagnostics;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public Task<ActionResult<TableBuilderResultViewModel>> Query([FromBody] ObservationQueryContext query)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var tableBuilderResultViewModel = _dataService.Query(query);

            stopwatch.Stop();
            _logger.LogDebug("Query {Query} executed in {Time} ms", query, stopwatch.Elapsed.TotalMilliseconds);

            return tableBuilderResultViewModel.HandleFailuresOrOk();
        }
    }
}