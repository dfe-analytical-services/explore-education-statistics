using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[ApiController]
public class ReleaseImageController : ControllerBase
{
    private readonly IReleaseFileService _releaseFileService;

    public ReleaseImageController(IReleaseFileService releaseFileService)
    {
        _releaseFileService = releaseFileService;
    }

    [HttpGet("releases/{releaseVersionId}/images/{fileId}")]
    public async Task<ActionResult> Stream(string releaseVersionId, string fileId)
    {
        if (Guid.TryParse(releaseVersionId, out var releaseVersionIdAsGuid) &&
            Guid.TryParse(fileId, out var fileIdAsGuid))
        {
            return await _releaseFileService
                .StreamFile(releaseVersionId: releaseVersionIdAsGuid, fileId: fileIdAsGuid)
                .HandleFailures();
        }

        return NotFound();
    }
}
