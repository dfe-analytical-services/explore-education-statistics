#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;

[Route("api")]
[ApiController]
[Authorize]
public class UserInvitesController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet("user-management/invites")]
    public async Task<ActionResult<List<PendingInviteViewModel>>> GetInvitedUsers()
    {
        return await userManagementService.ListPendingInvites().HandleFailuresOrOk();
    }

    [HttpPost("user-management/invites")]
    public async Task<ActionResult<User>> InviteUser(UserInviteCreateRequest request)
    {
        return await userManagementService.InviteUser(request).HandleFailuresOrOk();
    }

    [HttpDelete("user-management/invites/{email}")]
    public async Task<ActionResult> CancelUserInvite(string email)
    {
        return await userManagementService.CancelInvite(email).HandleFailuresOrNoContent();
    }
}
