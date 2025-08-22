#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;

[Route("api")]
[ApiController]
public class PublicationsSitemapController(IPublicationsSitemapService publicationsSitemapService) : ControllerBase
{
    [HttpGet("publications/sitemap-items")]
    public async Task<PublicationSitemapItemDto[]> GetSitemapItems(
        CancellationToken cancellationToken = default) =>
        await publicationsSitemapService.GetSitemapItems(cancellationToken);
}
