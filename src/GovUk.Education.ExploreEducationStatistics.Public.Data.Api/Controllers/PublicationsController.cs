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

    public PublicationsController(IPublicationService publicationService)
    {
        this._publicationService = publicationService;
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
        [FromQuery] PublicationsListRequest request)
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
    public async Task<ActionResult<PublicationSummaryViewModel>> GetPublication(Guid publicationId)
    {
        return await _publicationService
            .GetPublication(publicationId)
            .HandleFailuresOrOk();
    }
}
