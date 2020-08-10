using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> GetPublicationById(
            [Required] Guid publicationId)
        {
            return await _publicationService
                .GetViewModel(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpPost("api/publications")]
        public async Task<ActionResult<PublicationViewModel>> CreatePublication(
            SavePublicationViewModel publication)
        {
            return await _publicationService
                .CreatePublication(publication)
                .HandleFailuresOrOk();
        }

        [HttpPut("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            SavePublicationViewModel updatedPublication)
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
            List<PartialUpdateLegacyReleaseViewModel> legacyReleases)
        {
            return await _publicationService
                .PartialUpdateLegacyReleases(publicationId, legacyReleases)
                .HandleFailuresOrOk();
        }
    }
}