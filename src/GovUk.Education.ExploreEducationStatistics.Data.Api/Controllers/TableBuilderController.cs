using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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
            // TODO DFE-866 Remove ObservationQueryContext.StartYear and EndYear when frontend updated to use TimePeriod
            query.TimePeriod = new TimePeriodQuery();
            query.TimePeriod.StartYear = query.StartYear;
            query.TimePeriod.EndYear = query.EndYear;
            if (query.SubjectId == 17)
            {
                query.TimePeriod.StartCode = TimeIdentifier.CalendarYear;
                query.TimePeriod.EndCode = TimeIdentifier.CalendarYear;              
            }
            if (query.SubjectId <= 7)
            {
                query.TimePeriod.StartCode = TimeIdentifier.SixHalfTerms;
                query.TimePeriod.EndCode = TimeIdentifier.SixHalfTerms;
            }
            else
            {
                query.TimePeriod.StartCode = TimeIdentifier.AcademicYear;
                query.TimePeriod.EndCode = TimeIdentifier.AcademicYear;
            }

            var result = _dataService.Query(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }
    }
}