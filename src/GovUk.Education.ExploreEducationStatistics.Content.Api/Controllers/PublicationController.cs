#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;

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

        [HttpGet("publications")]
        public async Task<ActionResult<PaginatedListViewModel<PublicationSearchResultViewModel>>> GetPublications(
            [FromQuery] PublicationsGetRequest request)
        {
            return await _publicationService
                .GetPublications(
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

        public record PublicationsGetRequest(
            ReleaseType? ReleaseType,
            Guid? ThemeId,
            [MinLength(3)] string? Search,
            PublicationsSortBy? Sort,
            SortOrder? Order,
            [Range(1, int.MaxValue)] int Page = 1,
            [Range(1, int.MaxValue)] int PageSize = 10);
    }
}
