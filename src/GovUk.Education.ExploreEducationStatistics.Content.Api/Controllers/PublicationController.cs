#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[ApiController]
[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class PublicationController(IPublicationService publicationService) : ControllerBase
{
    [HttpGet("publicationInfos")]
    public async Task<ActionResult<IList<PublicationInfoViewModel>>> ListPublicationInfos(
        [FromQuery] GetPublicationInfosRequest request,
        CancellationToken cancellationToken = default
    ) => Ok(await publicationService.ListPublicationInfos(request.ThemeId, cancellationToken));
}
