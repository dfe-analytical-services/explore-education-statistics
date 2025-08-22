#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPreReleaseUserService
{
    Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseVersionId);

    Task<Either<ActionResult, PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId,
        List<string> emails);

    Task<Either<ActionResult, List<PreReleaseUserViewModel>>> InvitePreReleaseUsers(
        Guid releaseVersionId,
        List<string> emails);

    Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseVersionId,
        string email);

    Task<Either<ActionResult, Unit>> SendPreReleaseInviteEmail(
        Guid releaseVersionId,
        string email,
        bool isNewUser);

    Task MarkInviteEmailAsSent(UserReleaseInvite invite);
}
