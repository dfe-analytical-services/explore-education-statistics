#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

        [HttpGet("releases/{releaseId}/contributors")]
        public async Task<ActionResult<List<ContributorViewModel>>> ListReleaseContributors(
            Guid releaseId)
        {
            return await _releasePermissionService
                .ListReleaseContributors(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/contributor-invites")]
        public async Task<ActionResult<List<ContributorInviteViewModel>>> ListReleaseContributorInvites(
            Guid releaseId, [FromQuery] bool? accepted = null)
        {
            return await _releasePermissionService
                .ListReleaseContributorInvites(releaseId, accepted)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/contributors")]
        public async Task<ActionResult<List<ContributorViewModel>>> ListPublicationContributors(
            Guid publicationId)
        {
            return await _releasePermissionService
                .ListPublicationContributors(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId}/contributors")]
        public async Task<ActionResult> UpdateReleaseContributors(Guid releaseId, UpdateReleaseContributorsViewModel request)
        {
            return await _releasePermissionService
                .UpdateReleaseContributors(releaseId, request.UserIds)
                .HandleFailuresOr(result => new AcceptedResult());
        }

        [HttpDelete("publications/{publicationId}/users/{userId}/contributors")]
        public async Task<ActionResult> RemoveAllUserContributorPermissionsForPublication(
            Guid publicationId, Guid userId)
        {
            return await _releasePermissionService
                .RemoveAllUserContributorPermissionsForPublication(publicationId, userId)
                .HandleFailuresOr(result => new AcceptedResult());
        }
    }
}
