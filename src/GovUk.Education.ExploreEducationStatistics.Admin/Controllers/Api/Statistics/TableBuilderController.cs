using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;

        public TableBuilderController(IDataService<TableBuilderResultViewModel> dataService,
            ILogger<TableBuilderController> logger, IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService)
        {
            _dataService = dataService;
            _logger = logger;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<TableBuilderResultViewModel>> Query([FromUri, Required] ReleaseId releaseId,
            [FromBody] ObservationQueryContext query)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_ =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var tableBuilderResultViewModel = _dataService.Query(query, releaseId);

                    stopwatch.Stop();
                    _logger.LogDebug("Query {Query} executed in {Time} ms", query, stopwatch.Elapsed.TotalMilliseconds);

                    return tableBuilderResultViewModel;
                    
                })
                .HandleFailuresOr(Ok);
        }
    }
}