using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Debug;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly IGeographicDataService _geographicDataService;
        private readonly ICharacteristicDataService _characteristicDataService;

        public DebugController(IGeographicDataService geographicDataService,
            ICharacteristicDataService characteristicDataService)
        {
            _geographicDataService = geographicDataService;
            _characteristicDataService = characteristicDataService;
        }

        [HttpGet("report")]
        public async Task<ActionResult<DebugReport>> GetReport()
        {
            var geographicCount = _geographicDataService.Count();
            var characteristicCount = _characteristicDataService.Count();

            var counts = await Task.WhenAll(
                geographicCount, characteristicCount
            );

            return new DebugReport
            {
                geographicCount = counts[0],
                characteristicCount = counts[1]
            };
        }
    }
}