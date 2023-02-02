using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Authorize]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;
        private readonly IUserRoleService _roleService;

        public PublicationController(
            IPublicationService publicationService, 
            IUserRoleService roleService)
        {
            _publicationService = publicationService;
            _roleService = roleService;
        }

        [HttpGet("api/publications")]
        public async Task<ActionResult<List<PublicationViewModel>>> ListPublications(
            [FromQuery] Guid? topicId)
        {
            return await _publicationService
                .ListPublications(topicId)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publication-summaries")]
        public async Task<ActionResult<List<PublicationSummaryViewModel>>> ListPublicationSummaries()
        {
            return await _publicationService
                .ListPublicationSummaries()
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> GetPublication(
            [Required] Guid publicationId, [FromQuery] bool includePermissions = false)
        {
            return await _publicationService
                .GetPublication(publicationId, includePermissions)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publication/{publicationId:guid}/external-methodology")]
        public async Task<ActionResult<ExternalMethodologyViewModel>> GetExternalMethodology(Guid publicationId)
        {
            return await _publicationService.GetExternalMethodology(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpPut("api/publication/{publicationId:guid}/external-methodology")]
        public async Task<ActionResult<ExternalMethodologyViewModel>> UpdateExternalMethodology(
            Guid publicationId, ExternalMethodologySaveRequest updatedExternalMethodology)
        {
            return await _publicationService.UpdateExternalMethodology(publicationId, updatedExternalMethodology)
                .HandleFailuresOrOk();
        }

        [HttpDelete("api/publication/{publicationId:guid}/external-methodology")]
        public async Task<ActionResult<Unit>> RemoveExternalMethodology(
            Guid publicationId)
        {
            return await _publicationService.RemoveExternalMethodology(publicationId)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("api/publication/{publicationId:guid}/contact")]
        public async Task<ActionResult<ContactViewModel>> GetContact(
            Guid publicationId)
        {
            return await _publicationService.GetContact(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpPut("api/publication/{publicationId:guid}/contact")]
        public async Task<ActionResult<ContactViewModel>> UpdateContact(
            Guid publicationId, Contact updatedContact)
        {
            return await _publicationService.UpdateContact(publicationId, updatedContact)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/publication/{publicationId}/releases")]
        public async Task<ActionResult<PaginatedListViewModel<ReleaseSummaryViewModel>>> ListActiveReleases(
            [Required] Guid publicationId,
            [FromQuery, Range(1, double.PositiveInfinity)] int page = 1,
            [FromQuery, Range(0, double.PositiveInfinity)] int pageSize = 5,
            [FromQuery] bool? live = null,
            [FromQuery] bool includePermissions = false)
        {
            return await _publicationService
                .ListActiveReleasesPaginated(publicationId, page, pageSize, live, includePermissions)
                .HandleFailuresOrOk();
        }

        [HttpPost("api/publications")]
        public async Task<ActionResult<PublicationCreateViewModel>> CreatePublication(
            PublicationCreateRequest publication)
        {
            return await _publicationService
                .CreatePublication(publication)
                .HandleFailuresOrOk();
        }

        [HttpPut("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            PublicationSaveRequest updatedPublication)
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
        
        [HttpGet("api/publications/{publicationId}/roles")]
        public async Task<ActionResult<List<UserPublicationRoleViewModel>>> GetRoles(Guid publicationId)
        {
            return await _roleService
                .GetPublicationRolesForPublication(publicationId)
                .HandleFailuresOrOk();
        }
    }
}
