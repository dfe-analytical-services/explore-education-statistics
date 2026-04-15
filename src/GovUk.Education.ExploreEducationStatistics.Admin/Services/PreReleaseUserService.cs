#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseUserService(
    ContentDbContext contentDbContext,
    UsersAndRolesDbContext usersAndRolesDbContext,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService,
    IUserRepository userRepository,
    IUserPrereleaseRoleRepository userPrereleaseRoleRepository,
    IReleaseVersionRepository releaseVersionRepository
) : IPreReleaseUserService
{
    public async Task<List<PreReleaseUserViewModel>> GetAllPreReleaseUsers()
    {
        return
        [
            .. (
                await usersAndRolesDbContext
                    .Users.Join(
                        usersAndRolesDbContext.UserRoles,
                        user => user.Id,
                        userRole => userRole.UserId,
                        (user, userRole) => new { user, userRoleId = userRole.RoleId }
                    )
                    .Join(
                        usersAndRolesDbContext.Roles,
                        prev => prev.userRoleId,
                        role => role.Id,
                        (prev, role) =>
                            new
                            {
                                UserId = Guid.Parse(prev.user.Id),
                                Name = prev.user.FirstName + " " + prev.user.LastName,
                                prev.user.Email,
                                Role = role.Name,
                            }
                    )
                    .OrderBy(x => x.Name)
                    .Where(u => u.Role == Role.PrereleaseUser.GetEnumLabel())
                    .ToListAsync()
            ).Select(u => new PreReleaseUserViewModel
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email!,
            }),
        ];
    }

    public async Task<Either<ActionResult, List<PreReleaseUserSummaryViewModel>>> GetPreReleaseUsers(
        Guid releaseVersionId
    )
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(async _ =>
                (
                    await userPrereleaseRoleRepository
                        .Query(ResourceRoleFilter.All)
                        .WhereForReleaseVersion(releaseVersionId)
                        .Select(urr => urr.User.Email)
                        .Distinct()
                        .OrderBy(email => email)
                        .ToListAsync()
                )
                    .Select(email => new PreReleaseUserSummaryViewModel(email.ToLower()))
                    .ToList()
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
                var invitable = new List<string>();
                var alreadyAccepted = new List<string>();
                var alreadyInvited = new List<string>();

                foreach (var email in validEmails)
                {
                    var existingUser = await userRepository.FindUserByEmail(email);

                    if (existingUser is null)
                    {
                        invitable.Add(email);
                        continue;
                    }

                    var userAlreadyHasPrereleaseRole = await CheckUserAlreadyHasPrereleaseRoleOnReleaseVersion(
                        userId: existingUser.Id,
                        releaseVersionId: releaseVersionId
                    );

                    if (!userAlreadyHasPrereleaseRole)
                    {
                        invitable.Add(email);
                        continue;
                    }

                    if (existingUser.Active)
                    {
                        alreadyAccepted.Add(email);
                        continue;
                    }

                    alreadyInvited.Add(email);
                }

                if (invitable.Count == 0)
                {
                    return ValidationActionResult(NoInvitableEmails);
                }

                return new PreReleaseUserInvitePlan
                {
                    Invitable = invitable,
                    AlreadyAccepted = alreadyAccepted,
                    AlreadyInvited = alreadyInvited,
                };
            });
    }

    public async Task<Either<ActionResult, List<UserPrereleaseRoleViewModel>>> GetPrereleaseRolesForUser(Guid userId)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(_ => FindActiveUser(userId))
            .OnSuccess(async () =>
            {
                var allPrereleaseRoles = await userPrereleaseRoleRepository
                    .Query()
                    .AsNoTracking()
                    .WhereForUser(userId)
                    .Include(urr => urr.ReleaseVersion)
                        .ThenInclude(rv => rv.Release)
                            .ThenInclude(r => r.Publication)
                    .ToListAsync();

                var latestPrereleaseRoles = await allPrereleaseRoles
                    .ToAsyncEnumerable()
                    .Where(
                        async (uprr, cancellationToken) =>
                            await releaseVersionRepository.IsLatestReleaseVersion(
                                uprr.ReleaseVersionId,
                                cancellationToken
                            )
                    )
                    .OrderBy(uprr => uprr.ReleaseVersion.Release.Publication.Title)
                    .ThenBy(uprr => uprr.ReleaseVersion.Release.Year)
                    .ThenBy(uprr => uprr.ReleaseVersion.Release.TimePeriodCoverage)
                    .ToListAsync();

                return latestPrereleaseRoles
                    .Select(uprr => new UserPrereleaseRoleViewModel
                    {
                        Id = uprr.Id,
                        Publication = uprr.ReleaseVersion.Release.Publication.Title,
                        Release = uprr.ReleaseVersion.Release.Title,
                    })
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<PreReleaseUserSummaryViewModel>>> GrantPreReleaseAccessForMultipleUsers(
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
                    .Select(async (email, _, _) => await GrantPreReleaseAccess(releaseVersion, email))
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, Unit>> GrantPreReleaseAccess(Guid userId, Guid releaseId)
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .LatestReleaseVersion(releaseId: releaseId)
            .SingleOrNotFoundAsync()
            .OnSuccessCombineWith(_ => FindUserById(userId))
            .OnSuccess(tuple => (ReleaseVersion: tuple.Item1, User: tuple.Item2))
            .OnSuccessDo(tuple => userService.CheckCanAssignPrereleaseContactsToReleaseVersion(tuple.ReleaseVersion!))
            .OnSuccessDo(tuple =>
                ValidatePrereleaseRoleCanBeAdded(userId: userId, releaseVersionId: tuple.ReleaseVersion!.Id)
            )
            .OnSuccessVoid(tuple => GrantPreReleaseAccess(tuple.ReleaseVersion!, tuple.User));
    }

    public async Task<Either<ActionResult, Unit>> RemovePreReleaseRoleByCompositeKey(
        Guid releaseVersionId,
        string email
    )
    {
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return ValidationActionResult(InvalidEmailAddress);
        }

        return await CheckUserPrereleaseRoleExists(email: email, releaseVersionId: releaseVersionId)
            .OnSuccess(RemovePreReleaseRole);
    }

    public async Task<Either<ActionResult, Unit>> RemovePreReleaseRole(Guid userPrereleaseRoleId)
    {
        return await CheckUserPrereleaseRoleExists(userPrereleaseRoleId: userPrereleaseRoleId)
            .OnSuccess(RemovePreReleaseRole);
    }

    private async Task<PreReleaseUserSummaryViewModel> GrantPreReleaseAccess(
        ReleaseVersion releaseVersion,
        string email
    )
    {
        var activeUser = await userRepository.FindActiveUserByEmail(email);

        await contentDbContext.RequireTransaction(async () =>
        {
            var userToUse =
                activeUser
                ?? await userRepository.CreateOrUpdate(
                    email: email,
                    role: Role.PrereleaseUser,
                    createdById: userService.GetUserId()
                );

            await CreatePrereleaseRoleAndNotify(releaseVersion, userToUse);
        });

        return new PreReleaseUserSummaryViewModel(email);
    }

    private async Task GrantPreReleaseAccess(ReleaseVersion releaseVersion, User user)
    {
        await contentDbContext.RequireTransaction(async () =>
        {
            var userToUse = user.Active
                ? user
                : await userRepository.CreateOrUpdate(
                    email: user.Email,
                    role: Role.PrereleaseUser,
                    createdById: userService.GetUserId()
                );

            await CreatePrereleaseRoleAndNotify(releaseVersion, userToUse);
        });
    }

    private async Task CreatePrereleaseRoleAndNotify(ReleaseVersion releaseVersion, User user)
    {
        var createdUserPrereleaseRole = await userPrereleaseRoleRepository.Create(
            userId: user.Id,
            releaseVersionId: releaseVersion.Id,
            createdById: userService.GetUserId()
        );

        var shouldSendEmail = releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved;

        if (shouldSendEmail)
        {
            await userResourceRoleNotificationService.NotifyUserOfNewPreReleaseRole(createdUserPrereleaseRole.Id);
        }
    }

    private async Task<Either<ActionResult, Unit>> RemovePreReleaseRole(UserReleaseRole userPrereleaseRole) =>
        await userService
            .CheckCanAssignPrereleaseContactsToReleaseVersion(userPrereleaseRole.ReleaseVersion)
            .OnSuccessVoid(async _ =>
            {
                var removed = await userPrereleaseRoleRepository.RemoveById(userPrereleaseRole.Id);

                if (!removed)
                {
                    throw new InvalidOperationException(
                        $"Failed to remove User Pre-Release Role with ID {userPrereleaseRole.Id}"
                    );
                }
            });

    private async Task<Either<ActionResult, Unit>> ValidatePrereleaseRoleCanBeAdded(Guid userId, Guid releaseVersionId)
    {
        if (await CheckUserAlreadyHasPrereleaseRoleOnReleaseVersion(userId, releaseVersionId))
        {
            return ValidationActionResult(UserAlreadyHasResourceRole);
        }

        return Unit.Instance;
    }

    private async Task<bool> CheckUserAlreadyHasPrereleaseRoleOnReleaseVersion(Guid userId, Guid releaseVersionId) =>
        await userPrereleaseRoleRepository.UserHasPrereleaseRoleOnReleaseVersion(
            userId,
            releaseVersionId,
            ResourceRoleFilter.AllButExpired
        );

    private async Task<Either<ActionResult, UserReleaseRole>> CheckUserPrereleaseRoleExists(
        string email,
        Guid releaseVersionId
    ) =>
        await FindUserByEmail(email)
            .OnSuccess(user =>
                userPrereleaseRoleRepository
                    .Query(ResourceRoleFilter.All)
                    .WhereForUser(user.Id)
                    .WhereForReleaseVersion(releaseVersionId)
                    .Include(uprr => uprr.ReleaseVersion)
                    .SingleOrNotFoundAsync()
            );

    private async Task<Either<ActionResult, UserReleaseRole>> CheckUserPrereleaseRoleExists(
        Guid userPrereleaseRoleId
    ) =>
        await userPrereleaseRoleRepository
            .Query(ResourceRoleFilter.All)
            .Where(uprr => uprr.Id == userPrereleaseRoleId)
            .Include(uprr => uprr.ReleaseVersion)
            .SingleOrNotFoundAsync();

    private async Task<Either<ActionResult, User>> FindUserByEmail(string email) =>
        await userRepository.FindUserByEmail(email) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, User>> FindUserById(Guid userId) =>
        await userRepository.FindUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, User>> FindActiveUser(Guid userId) =>
        await userRepository.FindActiveUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());
}
