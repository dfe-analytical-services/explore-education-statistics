using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Debug;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly IFilterService _filterService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IObservationService _observationService;
        private readonly IReleaseService _releaseService;
        private readonly ISchoolService _schoolService;
        private readonly ISubjectService _subjectService;

        public DebugController(IFilterService filterService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IObservationService observationService,
            IReleaseService releaseService,
            ISchoolService schoolService,
            ISubjectService subjectService)
        {
            _filterService = filterService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _observationService = observationService;
            _releaseService = releaseService;
            _schoolService = schoolService;
            _subjectService = subjectService;
        }

        [HttpGet("report")]
        public async Task<ActionResult<DebugReport>> GetReport()
        {
            return new DebugReport
            {
                FilterCount = await _filterService.CountAsync(),
                IndicatorCount = await _indicatorService.CountAsync(),
                LocationCount = await _locationService.CountAsync(),
                ObservationCount = await _observationService.CountAsync(),
                ReleaseCount = await _releaseService.CountAsync(),
                SchoolCount = await _schoolService.CountAsync(),
                SubjectCount = await _subjectService.CountAsync()
            };
        }
    }
}