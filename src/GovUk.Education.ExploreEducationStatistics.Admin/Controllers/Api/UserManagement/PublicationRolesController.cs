#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
public class PublicationRolesController(IUserRoleService userRoleService) : ControllerBase
{
    [HttpGet("publications/{publicationId:guid}/publication-roles")]
    public async Task<ActionResult<List<UserPublicationRoleWithUserViewModel>>> ListPublicationRoles(
        Guid publicationId,
        CancellationToken cancellationToken = default
    )
    {
        return await userRoleService
            .GetPublicationRolesForPublication(publicationId, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost("users/{userId:guid}/publication-roles")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> AddPublicationRole(Guid userId, UserPublicationRoleCreateRequest request)
    {
        return await userRoleService
            .AddPublicationRole(userId, request.PublicationId, request.PublicationRole)
            .HandleFailuresOrOk();
    }

    [HttpPost("users/publication-roles/invite-drafter")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> InviteDrafter(
        UserDrafterRoleCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await userRoleService
            .InviteDrafter(
                email: request.Email,
                publicationId: request.PublicationId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    [HttpPatch("publications/{publicationId:guid}/update-drafters")]
    public async Task<ActionResult> UpdatePublicationDrafters(
        Guid publicationId,
        UpdatePublicationDraftersRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await userRoleService
            .UpdatePublicationDrafters(
                publicationId: publicationId,
                userIds: request.UserIds,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOr(_ => new AcceptedResult());
    }

    [HttpDelete("users/publication-roles/{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Unit>> DeleteUserPublicationRole(Guid id)
    {
        return await userRoleService.RemoveUserPublicationRole(id).HandleFailuresOrOk();
    }
}
