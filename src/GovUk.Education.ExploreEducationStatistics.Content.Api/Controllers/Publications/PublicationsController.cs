#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;

[Route("api")]
[ApiController]
public class PublicationsController(IPublicationsService publicationsService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}")]
    public async Task<ActionResult<PublicationDto>> GetPublication(
        string publicationSlug,
        CancellationToken cancellationToken = default)
    {
        return await publicationsService.GetPublication(
                publicationSlug,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}
