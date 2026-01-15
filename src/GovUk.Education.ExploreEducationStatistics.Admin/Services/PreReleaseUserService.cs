#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseUserService(
    ContentDbContext context,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService,
    IUserRepository userRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository
) : IPreReleaseUserService
{
    public async Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseVersionId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(async _ =>
                await userReleaseRoleRepository
                    .Query(ResourceRoleFilter.All)
                    .WhereForReleaseVersion(releaseVersionId)
                    .WhereRolesIn(ReleaseRole.PrereleaseViewer)
                    .Select(urr => new PreReleaseUserViewModel(urr.User.Email.ToLower()))
                    .Distinct()
                    .OrderBy(vm => vm.Email)
                    .ToListAsync()
            );
    }

    public async Task<Either<ActionResult, PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId,
        List<string> emails
    )
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(_ => EmailValidator.ValidateEmailAddresses(emails))
            .OnSuccess<ActionResult, List<string>, PreReleaseUserInvitePlan>(async validEmails =>
            {
                var plan = new PreReleaseUserInvitePlan();
                await validEmails
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async email =>
                    {
                        var existingUser = await userRepository.FindUserByEmail(email);

                        if (existingUser is null)
                        {
                            plan.Invitable.Add(email);
                            return;
                        }

                        var userHasPreReleaseRole = await userReleaseRoleRepository.UserHasRoleOnReleaseVersion(
                            userId: existingUser.Id,
                            releaseVersionId: releaseVersionId,
                            role: ReleaseRole.PrereleaseViewer,
                            resourceRoleFilter: ResourceRoleFilter.AllButExpired
                        );

                        if (!userHasPreReleaseRole)
                        {
                            plan.Invitable.Add(email);
                            return;
                        }

                        if (existingUser.Active)
                        {
                            plan.AlreadyAccepted.Add(email);
                            return;
                        }

                        plan.AlreadyInvited.Add(email);
                    });

                if (plan.Invitable.Count == 0)
                {
                    return ValidationActionResult(NoInvitableEmails);
                }

                return plan;
            });
    }

    public async Task<Either<ActionResult, List<PreReleaseUserViewModel>>> InvitePreReleaseUsers(
        Guid releaseVersionId,
        List<string> emails
    )
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccessCombineWith(_ => GetPreReleaseUsersInvitePlan(releaseVersionId, emails))
            .OnSuccess(async releaseVersionAndPlan =>
            {
                var (releaseVersion, plan) = releaseVersionAndPlan;

                return await plan
                    .Invitable.ToAsyncEnumerable()
                    .SelectAwait(async email => await InvitePreReleaseUser(releaseVersion, email))
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseVersionId, string email)
    {
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return ValidationActionResult(InvalidEmailAddress);
        }

        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(async () => await FindUserByEmail(email))
            .OnSuccess(async user => await FindUserPrereleaseRole(userId: user.Id, releaseVersionId: releaseVersionId))
            .OnSuccessVoid(async userPrereleaseRole => await userReleaseRoleRepository.Remove(userPrereleaseRole));
    }

    private async Task<PreReleaseUserViewModel> InvitePreReleaseUser(ReleaseVersion releaseVersion, string email)
    {
        var activeUser = await userRepository.FindActiveUserByEmail(email);

        await context.RequireTransaction(async () =>
        {
            var user =
                activeUser
                ?? await userRepository.CreateOrUpdate(
                    email: email,
                    role: Role.PrereleaseUser,
                    createdById: userService.GetUserId()
                );

            var createdUserReleaseRole = await userReleaseRoleRepository.Create(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                role: ReleaseRole.PrereleaseViewer,
                createdById: userService.GetUserId()
            );

            var shouldSendEmail = releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved;

            if (shouldSendEmail)
            {
                await userResourceRoleNotificationService.NotifyUserOfNewPreReleaseRole(createdUserReleaseRole.Id);
            }
        });

        return new PreReleaseUserViewModel(email);
    }

    private async Task<Either<ActionResult, User>> FindUserByEmail(string email)
    {
        var user = await userRepository.FindUserByEmail(email);

        return user is null ? new NotFoundResult() : user;
    }

    private async Task<Either<ActionResult, UserReleaseRole>> FindUserPrereleaseRole(Guid userId, Guid releaseVersionId)
    {
        var userPrereleaseRole = await userReleaseRoleRepository.GetByCompositeKey(
            userId: userId,
            releaseVersionId: releaseVersionId,
            role: ReleaseRole.PrereleaseViewer
        );

        return userPrereleaseRole is null ? new NotFoundResult() : userPrereleaseRole;
    }
}
