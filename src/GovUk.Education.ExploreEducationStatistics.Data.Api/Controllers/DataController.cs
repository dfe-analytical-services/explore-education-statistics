using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly ICombinedService _combinedService;

        public DataController(ICombinedService combinedService)
        {
            _combinedService = combinedService;
        }

        [HttpPost]
        public ActionResult<ResultViewModel> Query([FromBody] ObservationQueryContext query)
        {
            var result = _combinedService.Query(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }
    }
}