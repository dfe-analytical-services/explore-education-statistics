#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class ReleasePermissionsController : ControllerBase
{
    private readonly IReleasePermissionService _releasePermissionService;

    public ReleasePermissionsController(
        IReleasePermissionService releasePermissionService)
    {
        _releasePermissionService = releasePermissionService;
    }

    [HttpGet("releases/{releaseVersionId:guid}/roles")]
    public async Task<ActionResult<List<UserReleaseRoleSummaryViewModel>>> ListReleaseRoles(
        Guid releaseVersionId)
    {
        return await _releasePermissionService
            .ListReleaseRoles(releaseVersionId, new[] { ReleaseRole.Contributor, ReleaseRole.Approver })
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/invites")]
    public async Task<ActionResult<List<UserReleaseInviteViewModel>>> ListReleaseInvites(
        Guid releaseVersionId)
    {
        return await _releasePermissionService
            .ListReleaseInvites(releaseVersionId, new[] { ReleaseRole.Contributor, ReleaseRole.Approver })
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationId:guid}/contributors")]
    public async Task<ActionResult<List<UserReleaseRoleSummaryViewModel>>> ListPublicationContributors(
        Guid publicationId)
    {
        return await _releasePermissionService
            .ListPublicationContributors(publicationId)
            .HandleFailuresOrOk();
    }

    [HttpPut("releases/{releaseVersionId:guid}/contributors")]
    public async Task<ActionResult> UpdateReleaseContributors(Guid releaseVersionId, UpdateReleaseContributorsViewModel request)
    {
        return await _releasePermissionService
            .UpdateReleaseContributors(releaseVersionId, request.UserIds)
            .HandleFailuresOr(_ => new AcceptedResult());
    }

    [HttpDelete("publications/{publicationId:guid}/users/{userId:guid}/contributors")]
    public async Task<ActionResult> RemoveAllUserContributorPermissionsForPublication(
        Guid publicationId, Guid userId)
    {
        return await _releasePermissionService
            .RemoveAllUserContributorPermissionsForPublication(publicationId, userId)
            .HandleFailuresOr(_ => new AcceptedResult());
    }
}
