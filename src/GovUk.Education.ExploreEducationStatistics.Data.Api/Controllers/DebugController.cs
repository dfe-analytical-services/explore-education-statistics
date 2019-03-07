using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Debug;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly IGeographicDataService _geographicDataService;
        private readonly INationalCharacteristicDataService _nationalCharacteristicDataService;
        private readonly ILaCharacteristicDataService _laCharacteristicDataService;

        public DebugController(IGeographicDataService geographicDataService,
            INationalCharacteristicDataService nationalCharacteristicDataService,
            ILaCharacteristicDataService laCharacteristicDataService)
        {
            _geographicDataService = geographicDataService;
            _nationalCharacteristicDataService = nationalCharacteristicDataService;
            _laCharacteristicDataService = laCharacteristicDataService;
        }

        [HttpGet("report")]
        public async Task<ActionResult<DebugReport>> GetReport()
        {
            Task<int> geographicCount = _geographicDataService.Count();
            Task<int> nationalCharacteristicCount = _nationalCharacteristicDataService.Count();
            Task<int> laCharacteristicCount = _laCharacteristicDataService.Count();
            
            var counts = await Task.WhenAll(
                geographicCount, nationalCharacteristicCount, laCharacteristicCount
            );
            
            return new DebugReport
            {
                geographicCount = counts[0],
                nationalCharacteristicCount = counts[1],
                laCharacteristicCount = counts[2]
            };
        }
    }
}