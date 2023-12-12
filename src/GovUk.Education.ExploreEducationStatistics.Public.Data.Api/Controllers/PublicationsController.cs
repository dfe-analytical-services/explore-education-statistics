using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PublicationsController(IPublicationService PublicationService) : ControllerBase
{
    // GET: api/v1/publications
    [HttpGet]
    public async Task<ActionResult<PaginatedListViewModel<PublicationListViewModel>>> ListPublications(
        [FromQuery] PublicationsListRequest request)
    {
        return await PublicationService.ListPublications(
            page: request.Page,
            pageSize: request.PageSize,
            search: request.Search);
    }
}
