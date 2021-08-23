using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ReleaseImageController : ControllerBase
    {
        private readonly IReleaseFileService _releaseFileService;

        public ReleaseImageController(IReleaseFileService releaseFileService)
        {
            _releaseFileService = releaseFileService;
        }

        [HttpGet("releases/{releaseId}/images/{fileId}")]
        public async Task<ActionResult> Stream(string releaseId, string fileId)
        {
            if (Guid.TryParse(releaseId, out var releaseIdAsGuid) &&
                Guid.TryParse(fileId, out var fileIdAsGuid))
            {
                return await _releaseFileService
                    .StreamFile(releaseId: releaseIdAsGuid, fileId: fileIdAsGuid)
                    .HandleFailures();
            }

            return NotFound();
        }
    }
}
