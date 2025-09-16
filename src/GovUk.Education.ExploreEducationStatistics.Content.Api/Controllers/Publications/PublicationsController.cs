#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;

[Route("api")]
[ApiController]
public class PublicationsController(
    IPublicationReleasesService publicationReleasesService,
    IPublicationsService publicationsService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}")]
    public async Task<ActionResult<PublicationDto>> GetPublication(
        string publicationSlug,
        CancellationToken cancellationToken = default) =>
        await publicationsService.GetPublication(
                publicationSlug,
                cancellationToken)
            .HandleFailuresOrOk();

    [HttpGet("publications/{publicationSlug}/release-entries")]
    public async Task<ActionResult<PaginatedListViewModel<IPublicationReleaseEntryDto>>> GetPublicationReleases(
        [FromQuery] GetPublicationReleasesRequest request,
        CancellationToken cancellationToken = default) =>
        await publicationReleasesService.GetPublicationReleases(
                publicationSlug: request.PublicationSlug,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken)
            .HandleFailuresOrOk();
}
