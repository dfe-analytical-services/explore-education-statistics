using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<ActionResult<PaginatedListViewModel<PublicationListViewModel>>> ListPublications(
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
