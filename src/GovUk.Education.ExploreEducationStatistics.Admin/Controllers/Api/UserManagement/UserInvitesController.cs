using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        public UserInvitesController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("user-management/invites")]
        public async Task<ActionResult<List<UserViewModel>>> GetInvitedUsers()
        {
            return await _userManagementService
                .ListPendingInvites()
                .HandleFailuresOrOk();
        }

        [HttpPost("user-management/invites")]
        public async Task<ActionResult<UserInvite>> InviteUser(UserInviteRequest userInviteRequest)
        {
            return await _userManagementService
                .InviteUser(userInviteRequest.Email, User.Identity.Name,
                    userInviteRequest.RoleId)
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
