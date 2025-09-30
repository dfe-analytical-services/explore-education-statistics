#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class MethodologyController : ControllerBase
{
    private readonly IMethodologyService _methodologyService;

    public MethodologyController(IMethodologyService methodologyService)
    {
        _methodologyService = methodologyService;
    }

    [HttpGet("methodologies/{slug}")]
    public async Task<ActionResult<MethodologyVersionViewModel>> GetLatestMethodologyBySlug(
        string slug
    )
    {
        return await _methodologyService.GetLatestMethodologyBySlug(slug).HandleFailuresOrOk();
    }

    [HttpGet("methodologies/sitemap-items")]
    public async Task<ActionResult<List<MethodologySitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken = default
    ) => await _methodologyService.ListSitemapItems(cancellationToken).HandleFailuresOrOk();
}
