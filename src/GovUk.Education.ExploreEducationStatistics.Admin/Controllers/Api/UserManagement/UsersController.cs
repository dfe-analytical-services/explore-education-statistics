#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;

[ApiController]
[Route("api")]
[Authorize]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<List<UserViewModel>>> GetAllUsers()
    {
        return await userManagementService.ListAllUsers().HandleFailuresOrOk();
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<UserWithRolesViewModel>> GetUser(Guid userId)
    {
        return await userManagementService.GetUser(userId).HandleFailuresOrOk();
    }

    [HttpPut("users/{userId:guid}")]
    public async Task<ActionResult<Unit>> UpdateGlobalRole(Guid userId, UserGlobalRoleUpdateRequest request)
    {
        return await userManagementService
            .UpdateUserGlobalRole(userId, request.TargetGlobalRole!.Value)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// A BAU-only endpoint for deleting test users.
    /// </summary>
    [HttpDelete("users")]
    public async Task<ActionResult> DeleteUser([FromQuery] string email)
    {
        return await userManagementService.DeleteUser(email).HandleFailuresOrNoContent();
    }
}
