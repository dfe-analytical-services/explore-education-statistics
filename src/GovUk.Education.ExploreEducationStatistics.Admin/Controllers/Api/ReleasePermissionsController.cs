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

        [HttpGet("releases/{releaseId:guid}/roles")]
        public async Task<ActionResult<List<UserReleaseRoleSummaryViewModel>>> ListReleaseRoles(
            Guid releaseId)
        {
            return await _releasePermissionService
                .ListReleaseRoles(releaseId, new [] { ReleaseRole.Contributor, ReleaseRole.Approver })
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/invites")]
        public async Task<ActionResult<List<UserReleaseInviteViewModel>>> ListReleaseInvites(
            Guid releaseId)
        {
            return await _releasePermissionService
                .ListReleaseInvites(releaseId, new [] { ReleaseRole.Contributor, ReleaseRole.Approver })
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId:guid}/contributors")]
        public async Task<ActionResult<List<UserReleaseRoleSummaryViewModel>>> ListPublicationContributors(
            Guid publicationId)
        {
            return await _releasePermissionService
                .ListPublicationContributors(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId:guid}/contributors")]
        public async Task<ActionResult> UpdateReleaseContributors(Guid releaseId, UpdateReleaseContributorsViewModel request)
        {
            return await _releasePermissionService
                .UpdateReleaseContributors(releaseId, request.UserIds)
                .HandleFailuresOr(result => new AcceptedResult());
        }

        [HttpDelete("publications/{publicationId:guid}/users/{userId:guid}/contributors")]
        public async Task<ActionResult> RemoveAllUserContributorPermissionsForPublication(
            Guid publicationId, Guid userId)
        {
            return await _releasePermissionService
                .RemoveAllUserContributorPermissionsForPublication(publicationId, userId)
                .HandleFailuresOr(result => new AcceptedResult());
        }
    }
}
