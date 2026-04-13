using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[AllowAnonymous]
public class AssetController(IPublicBlobStorageService blobStorageService) : ControllerBase
{
    [HttpGet("assets/{fileName}")]
    public async Task<ActionResult<Stream>> Stream(string fileName, CancellationToken cancellationToken)
    {
        return await blobStorageService
            .GetDownloadStream(BlobContainers.Images, fileName, cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}
