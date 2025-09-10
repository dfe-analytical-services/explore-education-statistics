#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;

[Route("api")]
[ApiController]
public class ReleaseSearchableDocumentsController(IReleaseSearchableDocumentsService releaseSearchableDocumentsService)
    : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/latest/searchable")]
    public async Task<ActionResult<ReleaseSearchableDocumentDto>>
        GetLatestReleaseAsSearchableDocument(string publicationSlug) =>
        await releaseSearchableDocumentsService.GetLatestReleaseAsSearchableDocument(publicationSlug)
            .HandleFailuresOrOk();
}
