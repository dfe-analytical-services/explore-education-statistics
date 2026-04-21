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
    IUserPreReleaseRoleRepository userPreReleaseRoleRepository,
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
            .OnSuccess(userService.CheckCanAssignPreReleaseContactsToReleaseVersion)
            .OnSuccess(async _ =>
                (
                    await userPreReleaseRoleRepository
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
            .OnSuccess(userService.CheckCanAssignPreReleaseContactsToReleaseVersion)
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

                    var userAlreadyHasPreReleaseRole = await CheckUserAlreadyHasPreReleaseRoleOnReleaseVersion(
                        userId: existingUser.Id,
                        releaseVersionId: releaseVersionId
                    );

                    if (!userAlreadyHasPreReleaseRole)
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

    public async Task<Either<ActionResult, List<UserPreReleaseRoleViewModel>>> GetPreReleaseRolesForUser(Guid userId)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(_ => FindActiveUser(userId))
            .OnSuccess(async () =>
            {
                var allPreReleaseRoles = await userPreReleaseRoleRepository
                    .Query()
                    .AsNoTracking()
                    .WhereForUser(userId)
                    .Include(urr => urr.ReleaseVersion)
                        .ThenInclude(rv => rv.Release)
                            .ThenInclude(r => r.Publication)
                    .ToListAsync();

                var latestPreReleaseRoles = await allPreReleaseRoles
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

                return latestPreReleaseRoles
                    .Select(uprr => new UserPreReleaseRoleViewModel
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
            .OnSuccess(userService.CheckCanAssignPreReleaseContactsToReleaseVersion)
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
            .OnSuccessDo(tuple => userService.CheckCanAssignPreReleaseContactsToReleaseVersion(tuple.ReleaseVersion!))
            .OnSuccessDo(tuple =>
                ValidatePreReleaseRoleCanBeAdded(userId: userId, releaseVersionId: tuple.ReleaseVersion!.Id)
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

        return await CheckUserPreReleaseRoleExists(email: email, releaseVersionId: releaseVersionId)
            .OnSuccess(RemovePreReleaseRole);
    }

    public async Task<Either<ActionResult, Unit>> RemovePreReleaseRole(Guid userPreReleaseRoleId)
    {
        return await CheckUserPreReleaseRoleExists(userPreReleaseRoleId: userPreReleaseRoleId)
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

            await CreatePreReleaseRoleAndNotify(releaseVersion, userToUse);
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

            await CreatePreReleaseRoleAndNotify(releaseVersion, userToUse);
        });
    }

    private async Task CreatePreReleaseRoleAndNotify(ReleaseVersion releaseVersion, User user)
    {
        var createdUserPreReleaseRole = await userPreReleaseRoleRepository.Create(
            userId: user.Id,
            releaseVersionId: releaseVersion.Id,
            createdById: userService.GetUserId()
        );

        var shouldSendEmail = releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved;

        if (shouldSendEmail)
        {
            await userResourceRoleNotificationService.NotifyUserOfNewPreReleaseRole(createdUserPreReleaseRole.Id);
        }
    }

    private async Task<Either<ActionResult, Unit>> RemovePreReleaseRole(UserReleaseRole userPreReleaseRole) =>
        await userService
            .CheckCanAssignPreReleaseContactsToReleaseVersion(userPreReleaseRole.ReleaseVersion)
            .OnSuccessVoid(async _ =>
            {
                var removed = await userPreReleaseRoleRepository.RemoveById(userPreReleaseRole.Id);

                if (!removed)
                {
                    throw new InvalidOperationException(
                        $"Failed to remove User Pre-Release Role with ID {userPreReleaseRole.Id}"
                    );
                }
            });

    private async Task<Either<ActionResult, Unit>> ValidatePreReleaseRoleCanBeAdded(Guid userId, Guid releaseVersionId)
    {
        if (await CheckUserAlreadyHasPreReleaseRoleOnReleaseVersion(userId, releaseVersionId))
        {
            return ValidationActionResult(UserAlreadyHasResourceRole);
        }

        return Unit.Instance;
    }

    private async Task<bool> CheckUserAlreadyHasPreReleaseRoleOnReleaseVersion(Guid userId, Guid releaseVersionId) =>
        await userPreReleaseRoleRepository.UserHasPreReleaseRoleOnReleaseVersion(
            userId,
            releaseVersionId,
            ResourceRoleFilter.AllButExpired
        );

    private async Task<Either<ActionResult, UserReleaseRole>> CheckUserPreReleaseRoleExists(
        string email,
        Guid releaseVersionId
    ) =>
        await FindUserByEmail(email)
            .OnSuccess(user =>
                userPreReleaseRoleRepository
                    .Query(ResourceRoleFilter.All)
                    .WhereForUser(user.Id)
                    .WhereForReleaseVersion(releaseVersionId)
                    .Include(uprr => uprr.ReleaseVersion)
                    .SingleOrNotFoundAsync()
            );

    private async Task<Either<ActionResult, UserReleaseRole>> CheckUserPreReleaseRoleExists(
        Guid userPreReleaseRoleId
    ) =>
        await userPreReleaseRoleRepository
            .Query(ResourceRoleFilter.All)
            .Where(uprr => uprr.Id == userPreReleaseRoleId)
            .Include(uprr => uprr.ReleaseVersion)
            .SingleOrNotFoundAsync();

    private async Task<Either<ActionResult, User>> FindUserByEmail(string email) =>
        await userRepository.FindUserByEmail(email) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, User>> FindUserById(Guid userId) =>
        await userRepository.FindUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, User>> FindActiveUser(Guid userId) =>
        await userRepository.FindActiveUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());
}
