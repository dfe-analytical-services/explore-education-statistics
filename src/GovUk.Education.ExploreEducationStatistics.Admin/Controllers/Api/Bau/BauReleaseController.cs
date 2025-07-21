#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize]
public class BauReleaseController(IPublishingService publishingService) : ControllerBase
{
    /// <summary>
    /// Retry a combination of the Content and Publishing stages of the publishing workflow.
    /// </summary>
    /// <param name="releaseVersionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("bau/release/{releaseVersionId:guid}/publish/content")]
    public async Task<ActionResult<Unit>> RetryReleasePublishing(
        Guid releaseVersionId, CancellationToken cancellationToken)
    {
        return await publishingService
            .RetryReleasePublishing(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }
}
