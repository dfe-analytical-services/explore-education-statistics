using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class MethodologyImageController : ControllerBase
    {
        private readonly IMethodologyImageService _methodologyImageService;

        public MethodologyImageController(IMethodologyImageService methodologyImageService)
        {
            _methodologyImageService = methodologyImageService;
        }

        [HttpGet("methodologies/{methodologyVersionId}/images/{fileId}")]
        public async Task<ActionResult> Stream(string methodologyVersionId, string fileId)
        {
            if (Guid.TryParse(methodologyVersionId, out var methodologyVersionIdAsGuid) &&
                Guid.TryParse(fileId, out var fileIdAsGuid))
            {
                return await _methodologyImageService
                    .Stream(methodologyVersionId: methodologyVersionIdAsGuid, fileId: fileIdAsGuid)
                    .HandleFailures();
            }

            return NotFound();
        }
    }
}
