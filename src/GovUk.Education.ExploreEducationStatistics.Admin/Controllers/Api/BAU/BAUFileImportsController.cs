using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class BAUFileImportsController : Controller
    {
        private readonly IImportStatusService _importStatusService;

        public BAUFileImportsController(IImportStatusService importStatusService)
        {
            _importStatusService = importStatusService;
        }

        [Authorize(Policy = "CanCancelOngoingImports")]
        [HttpPost("release/{releaseId}/data/{dataFileName}/import/cancel")]
        public async Task<IActionResult> CancelFileImport([FromQuery] FileInfo file)
        {
            await _importStatusService.UpdateStatus(file.ReleaseId, file.DataFileName, IStatus.CANCELLED);
            return NoContent();
        }

        public class FileInfo
        {
            public Guid ReleaseId { get; set; }
            
            public string DataFileName { get; set; }
        }
    }
}