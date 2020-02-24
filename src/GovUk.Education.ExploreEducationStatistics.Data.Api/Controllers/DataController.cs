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
    public class DataController : ControllerBase
    {
        private readonly IDataService<ResultWithMetaViewModel> _dataService;

        public DataController(IDataService<ResultWithMetaViewModel> dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        public Task<ActionResult<ResultWithMetaViewModel>> Query([FromBody] ObservationQueryContext query)
        {
            return _dataService.Query(query).HandleFailuresOrOk();
        }
    }
}