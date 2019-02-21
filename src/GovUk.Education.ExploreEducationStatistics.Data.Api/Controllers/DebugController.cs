using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Debug;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly GeographicDataService _geographicDataService;
        private readonly NationalCharacteristicDataService _nationalCharacteristicDataService;
        private readonly LaCharacteristicDataService _laCharacteristicDataService;

        public DebugController(GeographicDataService geographicDataService,
            NationalCharacteristicDataService nationalCharacteristicDataService,
            LaCharacteristicDataService laCharacteristicDataService)
        {
            _geographicDataService = geographicDataService;
            _nationalCharacteristicDataService = nationalCharacteristicDataService;
            _laCharacteristicDataService = laCharacteristicDataService;
        }

        [HttpGet("report")]
        public ActionResult<DebugReport> GetReport()
        {
            return new DebugReport
            {
                geographicCount = _geographicDataService.Count(),
                nationalCharacteristicCount = _nationalCharacteristicDataService.Count(),
                laCharacteristicCount = _laCharacteristicDataService.Count()
            };
        }
    }
}