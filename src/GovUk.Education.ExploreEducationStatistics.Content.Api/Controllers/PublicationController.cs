#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IPublicationService _publicationService;

        public PublicationController(
            IPublicationCacheService publicationCacheService,
            IPublicationService publicationService)
        {
            _publicationCacheService = publicationCacheService;
            _publicationService = publicationService;
        }

        [HttpGet("publication-tree")]
        public async Task<ActionResult<IList<PublicationTreeThemeViewModel>>> GetPublicationTree(
            [FromQuery(Name = "publicationFilter")]
            PublicationTreeFilter? filter = null)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            return await _publicationCacheService
                .GetPublicationTree(filter.Value)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications")]
        public async Task<ActionResult<PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
            [FromQuery] PublicationsListRequest request)
        {
            return await _publicationService
                .ListPublications(
                    request.ReleaseType,
                    request.ThemeId,
                    request.Search,
                    request.Sort,
                    request.Order,
                    page: request.Page,
                    pageSize: request.PageSize)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{slug}/title")]
        public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
        {
            return await _publicationCacheService.GetPublication(slug)
                .OnSuccess(p => new PublicationTitleViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                })
                .HandleFailuresOrOk();
        }
    }
}
