using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[ApiController]
public class AssetController(IPublicBlobStorageService blobStorageService) : ControllerBase
{
    [HttpGet("asset/{assetType}/{fileName}")]
    public async Task<ActionResult<Stream>> Stream(
        AssetType assetType,
        string fileName,
        CancellationToken cancellationToken
    )
    {
        return assetType switch
        {
            AssetType.Image => await blobStorageService
                .GetDownloadStream(BlobContainers.Images, fileName, cancellationToken: cancellationToken)
                .HandleFailuresOrOk(),
            _ => NotFound(),
        };
    }
}
