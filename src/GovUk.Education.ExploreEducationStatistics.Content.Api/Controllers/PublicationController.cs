#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[ApiController]
[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class PublicationController : ControllerBase
{
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IPublicationService _publicationService;

    public PublicationController(
        IPublicationCacheService publicationCacheService,
        IPublicationService publicationService)
    {
        _publicationCacheService = publicationCacheService;
        _publicationService = publicationService;
    }

    [HttpGet("publication-tree")]
    public async Task<ActionResult<IList<PublicationTreeThemeViewModel>>> GetPublicationTree(
        [FromQuery(Name = "publicationFilter")]
        PublicationTreeFilter? filter = null)
    {
        if (filter == null)
        {
            return new BadRequestResult();
        }

        return await _publicationCacheService
            .GetPublicationTree(filter.Value)
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationId:guid}/summary")]
    public async Task<ActionResult<PublishedPublicationSummaryViewModel>> GetPublicationSummary(Guid publicationId)
    {
        return await _publicationService
            .GetSummary(publicationId)
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/{slug}/title")]
    public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
    {
        return await _publicationCacheService.GetPublication(slug)
            .OnSuccess(p => new PublicationTitleViewModel
            {
                Id = p.Id,
                Title = p.Title,
            })
            .HandleFailuresOrOk();
    }
    
    [HttpGet("publicationInfos")]
    public async Task<ActionResult<IList<PublicationInfoViewModel>>> ListPublicationInfos(
        [FromQuery] GetPublicationInfosRequest request, 
        CancellationToken cancellationToken = default) =>
        Ok(await _publicationService.ListPublicationInfos(request.ThemeId, cancellationToken));
}
