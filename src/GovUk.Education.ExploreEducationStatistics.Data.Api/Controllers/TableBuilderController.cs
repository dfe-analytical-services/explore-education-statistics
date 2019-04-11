using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;

        public TableBuilderController(ITableBuilderService tableBuilderService)
        {
            _tableBuilderService = tableBuilderService;
        }

        [HttpPost("geographic")]
        public ActionResult<TableBuilderResult> Query([FromBody] ObservationQueryContext query)
        {
            var result = _tableBuilderService.Query(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }
    }
}