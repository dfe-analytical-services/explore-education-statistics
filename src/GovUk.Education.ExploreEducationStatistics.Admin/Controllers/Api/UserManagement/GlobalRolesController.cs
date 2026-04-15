#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;

[ApiController]
[Route("api/global-roles")]
[Authorize]
public class GlobalRolesController(IUserRoleService userRoleService) : ControllerBase
{
    /// <summary>
    /// Provides a list of global roles that are available within the service
    /// </summary>
    /// <returns>Name and value representation of role</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<List<RoleViewModel>>> GetGlobalRoles()
    {
        return await userRoleService.GetAllGlobalRoles().HandleFailuresOrOk();
    }
}
