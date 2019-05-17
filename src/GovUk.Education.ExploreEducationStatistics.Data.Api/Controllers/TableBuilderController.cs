using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
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

        [HttpPost]
        public ActionResult<TableBuilderResultViewModel> Query([FromBody] ObservationQueryContext query)
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