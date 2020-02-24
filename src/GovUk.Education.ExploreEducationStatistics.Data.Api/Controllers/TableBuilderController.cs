using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
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
        public Task<ActionResult<TableBuilderResultViewModel>> Query([FromBody] ObservationQueryContext query)
        {
            return _dataService.Query(query).HandleFailuresOrOk();
        }
    }
}