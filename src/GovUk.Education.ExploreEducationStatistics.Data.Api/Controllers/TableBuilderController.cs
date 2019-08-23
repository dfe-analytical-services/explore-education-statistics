using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
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