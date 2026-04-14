#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[ApiController]
[Route("api")]
public class PublicationController(IPublicationService publicationService) : ControllerBase
{
    /// <summary>
    /// Used only by the Content.Search.FunctionApp (Search Docs Function App).
    /// </summary>
    [HttpGet("publicationInfos")]
    public async Task<IList<PublicationInfoViewModel>> GetPublicationInfos(
        [FromQuery] GetPublicationInfosRequest request,
        CancellationToken cancellationToken = default
    ) => await publicationService.ListPublicationInfos(request.ThemeId, cancellationToken);
}
