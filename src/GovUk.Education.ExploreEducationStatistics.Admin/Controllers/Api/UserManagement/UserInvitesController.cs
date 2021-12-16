#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        public UserInvitesController(IUserManagementService userManagementService,
            IReleaseInviteService releaseInviteService)
        {
            _userManagementService = userManagementService;
            _releaseInviteService = releaseInviteService;
        }

        [HttpGet("user-management/invites")]
        public async Task<ActionResult<List<UserViewModel>>> GetInvitedUsers()
        {
            return await _userManagementService
                .ListPendingInvites()
                .HandleFailuresOrOk();
        }

        [HttpPost("user-management/invites")]
        public async Task<ActionResult<UserInvite>> InviteUser(UserInviteViewModel userInviteViewModel)
        {
            return await _userManagementService
                .InviteUser(userInviteViewModel.Email, userInviteViewModel.RoleId)
                .HandleFailuresOrOk();
        }

        [HttpPost("user-management/publications/{publicationId:guid}/invites/contributor")]
        public async Task<ActionResult<Unit>> InviteContributor(Guid publicationId,
            ContributorInviteViewModel contributorInviteViewModel)
        {
            return await _releaseInviteService
                .InviteContributor(contributorInviteViewModel.Email, publicationId,
                    contributorInviteViewModel.ReleaseIds)
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
