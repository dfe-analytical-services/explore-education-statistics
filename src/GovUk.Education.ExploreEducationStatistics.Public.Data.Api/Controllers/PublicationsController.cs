using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/publications")]
public class PublicationsController : ControllerBase
{
    private readonly IPublicationService _publicationService;
    private readonly IDataSetService _dataSetService;

    public PublicationsController(IPublicationService publicationService, IDataSetService dataSetService)
    {
        _publicationService = publicationService;
        _dataSetService = dataSetService;
    }

    /// <summary>
    /// List publications
    /// </summary>
    /// <remarks>
    /// Lists details about publications with data available for querying.
    /// </remarks>
    [HttpGet]
    [Produces("application/json")]
    [SwaggerResponse(200, "The paginated list of publications", type: typeof(PaginatedPublicationListViewModel))]
    [SwaggerResponse(400)]
    public async Task<ActionResult<PaginatedPublicationListViewModel>> ListPublications(
        [FromQuery] ListPublicationsRequest request)
    {
        return await _publicationService
            .ListPublications(
                page: request.Page,
                pageSize: request.PageSize,
                search: request.Search)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a publication’s details
    /// </summary>
    /// <remarks>
    /// Get a specific publication's summary details.
    /// </remarks>
    [HttpGet("{publicationId:guid}")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The requested publication summary", type: typeof(PublicationSummaryViewModel))]
    [SwaggerResponse(400)]
    [SwaggerResponse(404)]
    // add other responses
    public async Task<ActionResult<PublicationSummaryViewModel>> GetPublication([SwaggerParameter("The ID of the publication.")] Guid publicationId)
    {
        return await _publicationService
            .GetPublication(publicationId)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// List a publication’s data sets
    /// </summary>
    /// <remarks>
    /// Lists summary details of all the data sets related to a publication.
    /// </remarks>
    [HttpGet("{publicationId:guid}/data-sets")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The paginated list of data sets", type: typeof(PaginatedDataSetViewModel))]
    [SwaggerResponse(400)]
    [SwaggerResponse(404)]
    public async Task<ActionResult<PaginatedDataSetViewModel>> ListDataSets(
        [FromQuery] ListDataSetsRequest request,
        [SwaggerParameter("The ID of the publication.")] Guid publicationId)
    {
        return await _dataSetService
            .ListDataSets(
                page: request.Page,
                pageSize: request.PageSize,
                publicationId)
            .HandleFailuresOrOk();
    }
}
