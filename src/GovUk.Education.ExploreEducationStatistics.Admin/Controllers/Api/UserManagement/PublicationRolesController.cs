#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;

[ApiController]
[Route("api")]
[Authorize]
public class PublicationRolesController(IUserRoleService userRoleService) : ControllerBase
{
    [HttpPost("users/{userId:guid}/publication-roles")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> AddPublicationRole(Guid userId, UserPublicationRoleCreateRequest request)
    {
        return await userRoleService
            .AddPublicationRole(userId, request.PublicationId, request.PublicationRole)
            .HandleFailuresOrOk();
    }

    [HttpDelete("users/publication-roles/{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> DeleteUserPublicationRole(Guid id)
    {
        return await userRoleService.RemoveUserPublicationRole(id).HandleFailuresOrOk();
    }
}
