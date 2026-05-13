#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RobotTests;

[ApiController]
[Route("api")]
[Authorize]
public class RobotTestsController(IUserRoleService userRoleService) : ControllerBase
{
    /// <summary>
    /// Removes all user resource roles for the given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("users/{userId:guid}/resource-roles")]
    [ProducesResponseType(204)]
    public async Task<ActionResult<Unit>> DeleteAllUserResourceRoles(Guid userId)
    {
        return await userRoleService.RemoveAllUserResourceRoles(userId).HandleFailuresOrNoContent();
    }
}
