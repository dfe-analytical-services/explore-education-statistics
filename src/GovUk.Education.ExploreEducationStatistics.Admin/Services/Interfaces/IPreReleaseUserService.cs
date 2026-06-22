#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPreReleaseUserService
{
    Task<List<PreReleaseUserViewModel>> GetAllPreReleaseUsers();

    Task<Either<ActionResult, List<PreReleaseUserSummaryViewModel>>> GetPreReleaseUsers(Guid releaseVersionId);

    Task<Either<ActionResult, PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId,
        PreReleaseUserInviteRequest emails
    );

    Task<Either<ActionResult, List<UserPreReleaseRoleViewModel>>> GetPreReleaseRolesForUser(Guid userId);

    Task<Either<ActionResult, List<PreReleaseUserSummaryViewModel>>> GrantPreReleaseAccessForMultipleUsers(
        Guid releaseVersionId,
        PreReleaseUserInviteRequest request
    );

    Task<Either<ActionResult, Unit>> GrantPreReleaseAccess(Guid userId, Guid releaseId);

    Task<Either<ActionResult, Unit>> RevokePreReleaseAccessByCompositeKey(
        Guid releaseVersionId,
        PreReleaseUserRemoveRequest request
    );

    Task<Either<ActionResult, Unit>> RevokePreReleaseAccessById(Guid userPreReleaseRoleId);
}
