#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserViewModel>>> GetAllUsers()
    {
        return await userManagementService.ListAllUsers().HandleFailuresOrOk();
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserViewModel>> GetUser(Guid userId)
    {
        return await userManagementService.GetUser(userId).HandleFailuresOrOk();
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<Unit>> UpdateUser(Guid userId, UserEditRequest request)
    {
        return await userManagementService.UpdateUser(userId.ToString(), request.RoleId).HandleFailuresOrOk();
    }

    /// <summary>
    /// A BAU-only endpoint for deleting test users.
    /// </summary>
    [HttpDelete("{email}")]
    [Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
    public async Task<ActionResult> DeleteUser(string email)
    {
        return await userManagementService.DeleteUser(email).HandleFailuresOrNoContent();
    }
}
