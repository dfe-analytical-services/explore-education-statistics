#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasePermissionsController : ControllerBase
    {
        private readonly IReleasePermissionService _releasePermissionService;

        public ReleasePermissionsController(
            IReleasePermissionService releasePermissionService)
        {
            _releasePermissionService = releasePermissionService;
        }

        [HttpGet("publications/{publicationId}/contributors")]
        public async Task<ActionResult<List<ContributorViewModel>>> GetReleaseContributors(
            Guid publicationId,
            [FromQuery] Guid releaseId)
        {
            return await _releasePermissionService
                .GetReleaseContributorPermissions(publicationId, releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/contributors")]
        public async Task<ActionResult<List<ContributorViewModel>>> GetPublicationContributors(
            Guid releaseId)
        {
            return await _releasePermissionService
                .GetPublicationContributorList(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPost("releases/{releaseId}/contributors")]
        public async Task<ActionResult> UpdateReleaseContributors(Guid releaseId, List<Guid> userIds)
        {
            return await _releasePermissionService
                .UpdateReleaseContributors(releaseId, userIds)
                .HandleFailuresOr(result => new AcceptedResult());
        }

        [HttpDelete("publications/{publicationId}/users/{userId}/contributors")]
        public async Task<ActionResult> RemoveUserContributorReleaseRolesForPublication(
            Guid publicationId, Guid userId)
        {
            return await _releasePermissionService
                .RemoveAllUserContributorPermissionsForPublication(publicationId, userId)
                .HandleFailuresOr(result => new AcceptedResult());
        }
    }
}
