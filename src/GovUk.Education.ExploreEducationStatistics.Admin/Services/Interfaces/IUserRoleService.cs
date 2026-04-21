#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserRoleService
{
    Task<Either<ActionResult, List<RoleViewModel>>> GetAllGlobalRoles();

    Task<Either<ActionResult, List<RoleViewModel>>> GetGlobalRolesForUser(string userId);

    Task<Either<ActionResult, Unit>> SetGlobalRoleForUser(string userId, string roleId);

    Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRolesForUser(Guid userId);

    Task<Either<ActionResult, List<UserPublicationRoleWithUserViewModel>>> GetPublicationRolesForPublication(
        Guid publicationId
    );

    Task<Either<ActionResult, Unit>> AddPublicationRole(Guid userId, Guid publicationId, PublicationRole role);

    Task<Either<ActionResult, Unit>> InviteDrafter(
        string email,
        Guid publicationId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid userPublicationRoleId);

    Task<Either<ActionResult, Unit>> RemoveAllUserResourceRoles(Guid userId);
}
