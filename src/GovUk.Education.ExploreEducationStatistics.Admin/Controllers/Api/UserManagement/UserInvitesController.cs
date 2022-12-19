#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class UserInvitesController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IReleaseInviteService _releaseInviteService;

        public UserInvitesController(
            IUserManagementService userManagementService,
            IReleaseInviteService releaseInviteService)
        {
            _userManagementService = userManagementService;
            _releaseInviteService = releaseInviteService;
        }

        [HttpGet("user-management/invites")]
        public async Task<ActionResult<List<PendingInviteViewModel>>> GetInvitedUsers()
        {
            return await _userManagementService
                .ListPendingInvites()
                .HandleFailuresOrOk();
        }

        [HttpPost("user-management/invites")]
        public async Task<ActionResult<UserInvite>> InviteUser(UserInviteCreateRequest request)
        {
            return await _userManagementService
                .InviteUser(request)
                .HandleFailuresOrOk();
        }

        [HttpPost("user-management/publications/{publicationId:guid}/invites/contributor")]
        public async Task<ActionResult<Unit>> InviteContributor(
            Guid publicationId,
            ContributorInviteCreateRequest request)
        {
            return await _releaseInviteService
                .InviteContributor(request.Email, publicationId, request.ReleaseIds)
                .HandleFailuresOrOk();
        }

        [HttpDelete("user-management/publications/{publicationId:guid}/release-invites/contributor")]
        public async Task<ActionResult<Unit>> RemoveContributorReleaseInvites(
            Guid publicationId,
            EmailViewModel emailViewModel)
        {
            return await _releaseInviteService
                .RemoveByPublication(emailViewModel.Email, publicationId,
                    ReleaseRole.Contributor)
                .HandleFailuresOrOk();
        }

        [HttpDelete("user-management/invites/{email}")]
        public async Task<ActionResult> CancelUserInvite(string email)
        {
            return await _userManagementService
                .CancelInvite(email)
                .HandleFailuresOrNoContent();
        }
    }
}
