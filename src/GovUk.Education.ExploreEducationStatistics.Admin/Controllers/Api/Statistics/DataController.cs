using System.Diagnostics;
using System.Web.Http;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
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
    public class DataController : ControllerBase
    {
        private readonly IDataService<ResultWithMetaViewModel> _dataService;
        private readonly ILogger _logger;

        public DataController(IDataService<ResultWithMetaViewModel> dataService,
            ILogger<DataController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<ResultWithMetaViewModel> Query([FromUri] ReleaseId releaseId,
            [FromBody] ObservationQueryContext query)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var resultWithMetaViewModel = _dataService.Query(query, releaseId);

            stopwatch.Stop();
            _logger.LogDebug("Query {Query} executed in {Time} ms", query, stopwatch.Elapsed.TotalMilliseconds);

            return resultWithMetaViewModel;
        }
    }
}