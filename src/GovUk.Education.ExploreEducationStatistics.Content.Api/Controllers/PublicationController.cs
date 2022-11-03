#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService.
    PublicationsSortBy;

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
        public async Task<ActionResult<List<PublicationSearchResultViewModel>>> GetPublications(
            [FromQuery] PublicationsGetRequest request)
        {
            var sort = request.Sort ?? (request.Search == null ? Title : Relevance);
            var order = request.Order ?? (sort == Title ? Asc : Desc);

            return await _publicationService
                .GetPublications(
                    request.ReleaseType,
                    request.ThemeId,
                    request.Search,
                    sort,
                    order,
                    offset: request.Offset,
                    limit: request.Limit)
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
            [Range(0, int.MaxValue)] int Offset = 0,
            [Range(1, int.MaxValue)] int Limit = 10);
    }
}
