using System;
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
        private readonly IDataService<TableBuilderResultViewModel> _dataService;

        public TableBuilderController(IDataService<TableBuilderResultViewModel> dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        [Obsolete("TODO DFE-1277 Remove when table tool switches to new endpoint")]
        public ActionResult<ResultViewModel> Query([FromBody] ObservationQueryContext query)
        {
            var tableResultViewModel = _dataService.Query(query);
            return new ResultViewModel
            {
                Footnotes = tableResultViewModel.SubjectMeta.Footnotes,
                TimePeriodRange = tableResultViewModel.SubjectMeta.TimePeriodRange,
                Result = tableResultViewModel.Results
            };
        }

        [HttpPost("new")]
        public ActionResult<TableBuilderResultViewModel> QueryNew([FromBody] ObservationQueryContext query)
        {
            return _dataService.Query(query);
        }
    }
}