using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class PublicationsController : ControllerBase
{
    private readonly IPublicationService _publicationService;

    public PublicationsController(IPublicationService publicationService)
    {
        this._publicationService = publicationService;
    }

    /// <summary>
    /// List Publications
    /// </summary>
    /// <remarks>
    /// Lists all of the publications which are published to the Public API. It will paginate
    /// the response based on the request parameters. By default, the results will be ordered
    /// by the title of the publications in ascending order. If a search term is supplied, it will
    /// order the results by how well their titles match the search term.
    /// </remarks>
    [HttpGet]
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
}
