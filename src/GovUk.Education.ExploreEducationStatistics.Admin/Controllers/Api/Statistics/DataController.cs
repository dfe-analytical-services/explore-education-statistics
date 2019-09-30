using System.Web.Http;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReleaseId = System.Guid;
 
namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly IDataService<ResultWithMetaViewModel> _dataService;
 
        public DataController(IDataService<ResultWithMetaViewModel> dataService)
        {
            _dataService = dataService;
        }
 
        [HttpPost]
        public ActionResult<ResultWithMetaViewModel> Query([FromUri] ReleaseId release, [FromBody] ObservationQueryContext query)
        {
            return _dataService.Query(query, release);
        }
    }
}