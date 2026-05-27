#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
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
public class PreReleaseUsersController(IPreReleaseUserService preReleaseUserService) : ControllerBase
{
    [HttpGet("pre-release/users")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [Authorize(Policy = nameof(SecurityPolicies.CanManageUsersOnSystem))]
    public async Task<ActionResult<List<PreReleaseUserViewModel>>> GetAllPreReleaseUsers()
    {
        return await preReleaseUserService.GetAllPreReleaseUsers();
    }

    [HttpDelete("pre-release/roles/{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> RevokePreReleaseAccessById(Guid id)
    {
        return await preReleaseUserService.RevokePreReleaseAccessById(id).HandleFailuresOrOk();
    }

    [HttpGet("pre-release/release-versions/{releaseVersionId:guid}/users")]
    public async Task<ActionResult<List<PreReleaseUserSummaryViewModel>>> GetPreReleaseUsers(Guid releaseVersionId)
    {
        return await preReleaseUserService.GetPreReleaseUsers(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpPost("pre-release/release-versions/{releaseVersionId:guid}/users/invite-plan")]
    public async Task<ActionResult<PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId,
        [FromBody] PreReleaseUserInviteRequest request
    )
    {
        return await preReleaseUserService.GetPreReleaseUsersInvitePlan(releaseVersionId, request).HandleFailuresOrOk();
    }

    [HttpPost("pre-release/release-versions/{releaseVersionId:guid}/users")]
    public async Task<ActionResult<List<PreReleaseUserSummaryViewModel>>> GrantPreReleaseAccess(
        Guid releaseVersionId,
        [FromBody] PreReleaseUserInviteRequest request
    )
    {
        return await preReleaseUserService
            .GrantPreReleaseAccessForMultipleUsers(releaseVersionId, request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("pre-release/release-versions/{releaseVersionId:guid}/users/by-email")]
    public async Task<ActionResult> RevokePreReleaseAccessByEmail(
        Guid releaseVersionId,
        [FromBody] PreReleaseUserRemoveRequest request
    )
    {
        return await preReleaseUserService
            .RevokePreReleaseAccessByCompositeKey(releaseVersionId, request)
            .HandleFailuresOrNoContent();
    }

    [HttpPost("pre-release/releases/{releaseId:guid}/users/{userId:guid}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> GrantPreReleaseAccess(Guid releaseId, Guid userId)
    {
        return await preReleaseUserService
            .GrantPreReleaseAccess(userId: userId, releaseId: releaseId)
            .HandleFailuresOrOk();
    }
}
