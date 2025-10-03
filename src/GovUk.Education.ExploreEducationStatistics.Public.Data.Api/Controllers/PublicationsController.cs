using System.Net.Mime;
using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/publications")]
public class PublicationsController(IPublicationService publicationService, IDataSetService dataSetService)
    : ControllerBase
{
    /// <summary>
    /// List publications
    /// </summary>
    /// <remarks>
    /// Lists details about publications with data available for querying.
    /// </remarks>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The paginated list of publications.", type: typeof(PublicationPaginatedListViewModel))]
    [SwaggerResponse(400, type: typeof(ValidationProblemViewModel))]
    public async Task<ActionResult<PublicationPaginatedListViewModel>> ListPublications(
        [FromQuery] PublicationListRequest request,
        CancellationToken cancellationToken
    )
    {
        return await publicationService
            .ListPublications(
                page: request.Page,
                pageSize: request.PageSize,
                search: request.Search,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a publication's details
    /// </summary>
    /// <remarks>
    /// Get a publication's summary details.
    /// </remarks>
    [HttpGet("{publicationId:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The requested publication summary.", type: typeof(PublicationSummaryViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    // add other responses
    public async Task<ActionResult<PublicationSummaryViewModel>> GetPublication(
        [SwaggerParameter("The ID of the publication.")] Guid publicationId,
        CancellationToken cancellationToken
    )
    {
        return await publicationService
            .GetPublication(publicationId: publicationId, cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// List a publication's data sets
    /// </summary>
    /// <remarks>
    /// Lists summary details of all the data sets related to a publication.
    /// </remarks>
    [HttpGet("{publicationId:guid}/data-sets")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The paginated list of data sets.", type: typeof(DataSetPaginatedListViewModel))]
    [SwaggerResponse(400, type: typeof(ValidationProblemViewModel))]
    public async Task<ActionResult<DataSetPaginatedListViewModel>> ListPublicationDataSets(
        [FromQuery] DataSetListRequest request,
        [SwaggerParameter("The ID of the publication.")] Guid publicationId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetService
            .ListDataSets(
                page: request.Page,
                pageSize: request.PageSize,
                publicationId: publicationId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }
}
