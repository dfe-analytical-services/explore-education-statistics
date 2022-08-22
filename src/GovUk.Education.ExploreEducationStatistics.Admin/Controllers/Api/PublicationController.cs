using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LegacyReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.LegacyReleaseViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Publications once the current Crud publication controller is removed
    [Authorize]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        [HttpGet("api/me/publications")]
        public async Task<ActionResult<List<MyPublicationViewModel>>> GetMyPublications(
            [FromQuery(Name = "topicId"), Required] Guid topicId)
        {
            return await _publicationService
                .GetMyPublicationsAndReleasesByTopic(topicId)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publication-summaries")]
        public async Task<ActionResult<List<PublicationSummaryViewModel>>> ListPublicationSummaries()
        {
            return await _publicationService
                .ListPublicationSummaries()
                .HandleFailuresOrOk();
        }

        [HttpGet("api/me/publication/{publicationId}")]
        public async Task<ActionResult<MyPublicationViewModel>> GetMyPublication(
            [FromRoute(Name = "publicationId"), Required] Guid publicationId)
        {
            return await _publicationService
                .GetMyPublication(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> GetPublication(
            [Required] Guid publicationId)
        {
            return await _publicationService
                .GetPublication(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publication/{publicationId}/releases")]
        public async Task<ActionResult<PaginatedListViewModel<ReleaseListItemViewModel>>> ListActiveReleases(
            [Required] Guid publicationId,
            [FromQuery, Range(1, double.PositiveInfinity)] int page = 1,
            [FromQuery, Range(0, double.PositiveInfinity)] int pageSize = 5,
            [FromQuery] bool? live = null)
        {
            return await _publicationService
                .ListActiveReleasesPaginated(publicationId, page, pageSize, live)
                .HandleFailuresOrOk();
        }

        [HttpPost("api/publications")]
        public async Task<ActionResult<PublicationViewModel>> CreatePublication(
            PublicationSaveViewModel publication)
        {
            return await _publicationService
                .CreatePublication(publication)
                .HandleFailuresOrOk();
        }

        [HttpPut("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            PublicationSaveViewModel updatedPublication)
        {
            return await _publicationService
                .UpdatePublication(publicationId, updatedPublication)
                .HandleFailuresOrOk();
        }

        /// Partially update the publication's legacy releases.
        /// Only legacy releases with matching ids will be updated,
        /// and only non-null fields will be updated.
        /// This is useful for bulk updates e.g. re-ordering.
        [HttpPatch("api/publications/{publicationId}/legacy-releases")]
        public async Task<ActionResult<List<LegacyReleaseViewModel>>> PartialUpdateLegacyReleases(
            Guid publicationId,
            List<LegacyReleasePartialUpdateViewModel> legacyReleases)
        {
            return await _publicationService
                .PartialUpdateLegacyReleases(publicationId, legacyReleases)
                .HandleFailuresOrOk();
        }
    }
}
