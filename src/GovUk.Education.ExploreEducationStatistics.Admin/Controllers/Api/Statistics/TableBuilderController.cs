using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class TableBuilderController: ControllerBase
    {
        private readonly IDataService<TableBuilderResultViewModel> _dataService;

        public TableBuilderController(IDataService<TableBuilderResultViewModel> dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        public ActionResult<TableBuilderResultViewModel> Query([FromBody] ObservationQueryContext query)
        {
            return _dataService.Query(query);
        }
    }
}