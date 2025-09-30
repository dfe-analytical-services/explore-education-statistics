using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
public class SecureBlobDownloadController(IPrivateBlobStorageService blobService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("download-blob")]
    public async Task<ActionResult> StreamWithToken(
        [FromQuery] string token,
        CancellationToken cancellationToken
    )
    {
        var decodedToken = BlobDownloadToken.FromBase64JsonString(token);

        return await blobService.StreamWithToken(decodedToken, cancellationToken).HandleFailures();
    }
}
