using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly IDataService<ResultViewModel> _dataService;

        public TableBuilderController(IDataService<ResultViewModel> dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        public ActionResult<ResultViewModel> Query([FromBody] ObservationQueryContext query)
        {
            var result = _dataService.Query(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }
    }
}