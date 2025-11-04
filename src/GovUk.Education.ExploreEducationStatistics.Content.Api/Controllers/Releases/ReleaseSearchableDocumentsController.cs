#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;

/// <summary>
/// Provides an endpoint to retrieve a searchable document representation of releases. This is used by the
/// Search Docs Function App to create searchable documents for Azure AI Search indexing.
/// </summary>
[Route("api")]
[ApiController]
public class ReleaseSearchableDocumentsController(IReleaseSearchableDocumentsService releaseSearchableDocumentsService)
    : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/latest/searchable")]
    public async Task<ActionResult<ReleaseSearchableDocumentDto>> GetLatestReleaseAsSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        await releaseSearchableDocumentsService
            .GetLatestReleaseAsSearchableDocument(publicationSlug, cancellationToken)
            .HandleFailuresOrOk();
}
