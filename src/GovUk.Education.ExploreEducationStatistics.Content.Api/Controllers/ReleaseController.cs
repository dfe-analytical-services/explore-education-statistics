#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

/// <summary>
/// This controller contains an endpoint retained after the Release page redesign, which continues to serve
/// the Public site frontend Data Catalogue page. Newer release-related controllers can be found in the
/// <see cref="GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases"/> namespace.
/// </summary>
[ApiController]
[Route("api")]
public class ReleaseController(IReleaseService releaseService) : ControllerBase
{
    /// <summary>
    /// Returns a list of releases for a given publication, used by the Public site frontend Data Catalogue page
    /// when a publication is selected.
    /// </summary>
    [HttpGet("publications/{publicationSlug}/releases")]
    public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListReleases(string publicationSlug) =>
        await releaseService.List(publicationSlug).HandleFailuresOrOk();
}
