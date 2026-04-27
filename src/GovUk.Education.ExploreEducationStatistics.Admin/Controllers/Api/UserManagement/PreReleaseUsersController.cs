#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;

[ApiController]
[Route("api/pre-release")]
[Authorize]
public class PreReleaseUsersController(IPreReleaseUserService preReleaseUserService) : ControllerBase
{
    [HttpGet("users")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [Authorize(Roles = nameof(SecurityPolicies.CanManageUsersOnSystem))]
    public async Task<ActionResult<List<PreReleaseUserViewModel>>> GetAllPreReleaseUsers()
    {
        return await preReleaseUserService.GetAllPreReleaseUsers();
    }

    [HttpDelete("roles/{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> RemovePreReleaseRoleById(Guid id)
    {
        return await preReleaseUserService.RemovePreReleaseRole(id).HandleFailuresOrOk();
    }

    [HttpGet("release-versions/{releaseVersionId:guid}/users")]
    public async Task<ActionResult<List<PreReleaseUserSummaryViewModel>>> GetPreReleaseUsers(Guid releaseVersionId)
    {
        return await preReleaseUserService.GetPreReleaseUsers(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpPost("release-versions/{releaseVersionId:guid}/users/invite-plan")]
    public async Task<ActionResult<PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId,
        [FromBody] PreReleaseUserInviteRequest request
    )
    {
        return await preReleaseUserService
            .GetPreReleaseUsersInvitePlan(releaseVersionId, request.Emails)
            .HandleFailuresOrOk();
    }

    [HttpPost("release-versions/{releaseVersionId:guid}/users")]
    public async Task<ActionResult<List<PreReleaseUserSummaryViewModel>>> GrantPreReleaseAccess(
        Guid releaseVersionId,
        [FromBody] PreReleaseUserInviteRequest request
    )
    {
        return await preReleaseUserService
            .GrantPreReleaseAccessForMultipleUsers(releaseVersionId, request.Emails)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release-versions/{releaseVersionId:guid}/users/by-email")]
    public async Task<ActionResult> RemovePreReleaseRoleByEmail(
        Guid releaseVersionId,
        [FromBody] PreReleaseUserRemoveRequest request
    )
    {
        return await preReleaseUserService
            .RemovePreReleaseRoleByCompositeKey(releaseVersionId, request.Email)
            .HandleFailuresOrNoContent();
    }

    [HttpPost("releases/{releaseId:guid}/users/{userId:guid}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> GrantPreReleaseAccess(Guid releaseId, Guid userId)
    {
        return await preReleaseUserService
            .GrantPreReleaseAccess(userId: userId, releaseId: releaseId)
            .HandleFailuresOrOk();
    }
}
