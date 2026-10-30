#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.DataSets;

/// <summary>
/// Provides endpoints to retrieve a searchable document representation of data sets. These are used by the
/// Search Docs Function App to create data set documents for Azure AI Search indexing.
/// </summary>
[Route("api")]
[ApiController]
public class DataSetSearchableDocumentsController(IDataSetSearchableDocumentsService dataSetSearchableDocumentsService)
    : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/data-sets/searchable")]
    public async Task<ActionResult<DataSetSearchableDocumentDto[]>> GetReleaseDataSetSearchableDocuments(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await dataSetSearchableDocumentsService
            .GetReleaseDataSetSearchableDocuments(publicationSlug, releaseSlug, cancellationToken)
            .HandleFailuresOrOk();

    [HttpGet("publications/{publicationSlug}/data-sets/searchable")]
    public async Task<ActionResult<DataSetSearchableDocumentDto[]>> GetPublicationDataSetSearchableDocuments(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        await dataSetSearchableDocumentsService
            .GetPublicationDataSetSearchableDocuments(publicationSlug, cancellationToken)
            .HandleFailuresOrOk();

    [HttpGet("data-sets/searchable/keys")]
    public async Task<ActionResult<DataSetSearchableDocumentKeyDto[]>> GetDataSetSearchableDocumentKeys(
        CancellationToken cancellationToken = default
    ) =>
        await dataSetSearchableDocumentsService
            .GetDataSetSearchableDocumentKeys(cancellationToken)
            .HandleFailuresOrOk();
}
