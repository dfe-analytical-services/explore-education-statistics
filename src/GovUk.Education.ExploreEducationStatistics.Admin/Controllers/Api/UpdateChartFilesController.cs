using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class UpdateChartFilesController : ControllerBase
    {
        private readonly IUpdateChartFilesService _updateChartFilesService;

        public UpdateChartFilesController(IUpdateChartFilesService updateChartFilesService)
        {
            _updateChartFilesService = updateChartFilesService;
        }
        
        [HttpPost("charts/change-chart-filenames-to-guids")]
        public async Task<ActionResult<Unit>> UpdateChartFilenamesToIds()
        {
            return await _updateChartFilesService
                .UpdateChartFiles()
                .HandleFailuresOrOk();
        }
    }
}