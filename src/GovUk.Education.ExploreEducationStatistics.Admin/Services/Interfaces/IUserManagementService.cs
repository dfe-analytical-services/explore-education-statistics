#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserManagementService
{
    Task<Either<ActionResult, UserWithRolesViewModel>> GetUser(Guid id);

    Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers();

    Task<Either<ActionResult, List<PendingInviteViewModel>>> ListPendingInvites();

    Task<Either<ActionResult, User>> InviteUser(UserInviteCreateRequest request);

    Task<Either<ActionResult, Unit>> CancelInvite(string email);

    Task<Either<ActionResult, Unit>> UpdateUserGlobalRole(Guid userId, GlobalRoles.Role targetGlobalRole);

    Task<Either<ActionResult, Unit>> DeleteUser(string email);
}
