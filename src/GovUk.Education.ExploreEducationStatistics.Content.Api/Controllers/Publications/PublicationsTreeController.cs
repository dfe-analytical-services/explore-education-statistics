#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;

[Route("api")]
[ApiController]
public class PublicationsTreeController(IPublicationsTreeService publicationsTreeService) : ControllerBase
{
    [HttpGet("publications/tree")]
    public async Task<ActionResult<PublicationsTreeThemeDto[]>> GetPublicationsTree(
        [FromQuery] [Required] PublicationsTreeFilter filter,
        CancellationToken cancellationToken = default
    ) => await publicationsTreeService.GetPublicationsTreeFiltered(filter, cancellationToken);
}
