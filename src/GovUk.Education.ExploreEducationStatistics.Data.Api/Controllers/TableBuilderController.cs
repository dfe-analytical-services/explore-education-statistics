using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly IDataService<TableResultViewModel> _dataService;

        public TableBuilderController(IDataService<TableResultViewModel> dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        public ActionResult<ResultViewModel> Query([FromBody] ObservationQueryContext query)
        {
            // TODO DFE-1277 Remove when table tool switches to new endpoint
            var tableResultViewModel = _dataService.Query(query);
            return new ResultViewModel
            {
                Footnotes = tableResultViewModel.Footnotes,
                TimePeriodRange = tableResultViewModel.TimePeriodRange,
                Result = tableResultViewModel.Result
            };
        }

        [HttpPost("new")]
        public ActionResult<TableResultViewModel> QueryNew([FromBody] ObservationQueryContext query)
        {
            return _dataService.Query(query);
        }
    }
}