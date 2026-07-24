#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using UserPreReleaseRoleCreateDto = GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPreReleaseRoleRepository.UserPreReleaseRoleCreateDto;
using UserPublicationRoleCreateDto = GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPublicationRoleRepository.UserPublicationRoleCreateDto;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserManagementService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    ContentDbContext contentDbContext,
    IUserRoleService userRoleService,
    IUserRepository userRepository,
    IUserService userService,
    IUserPreReleaseRoleRepository userPreReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IPreReleaseUserService preReleaseUserService,
    UserManager<ApplicationUser> userManager
) : IUserManagementService
{
    public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                var activeUsers = await contentDbContext.Users.AsNoTracking().Where(user => user.Active).ToListAsync();

                return activeUsers
                    .Select(user => new UserViewModel
                    {
                        Id = user.Id,
                        Name = user.DisplayName,
                        Email = user.Email,
                        GlobalRole = user.GetGlobalRole(),
                    })
                    .OrderBy(user => user.Name)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, UserWithRolesViewModel>> GetUser(Guid id)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await GetActiveUserById(id)
                    .OnSuccess(async user =>
                    {
                        return await userRoleService
                            .GetPublicationRolesForUser(id)
                            .OnSuccessCombineWith(_ => preReleaseUserService.GetPreReleaseRolesForUser(id))
                            .OnSuccess(tuple =>
                            {
                                var (publicationRoles, preReleaseRoles) = tuple;

                                return new UserWithRolesViewModel
                                {
                                    Id = id,
                                    Name = user.DisplayName,
                                    Email = user.Email!,
                                    GlobalRole = user.GetGlobalRole(),
                                    UserPublicationRoles = publicationRoles,
                                    UserPreReleaseRoles = preReleaseRoles,
                                };
                            });
                    });
            });
    }

    public async Task<Either<ActionResult, List<PendingInviteViewModel>>> ListPendingInvites()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                var pendingUserInvites = await contentDbContext
                    .Users.AsNoTracking()
                    .WhereInvitePending()
                    .Select(u => new
                    {
                        UserId = u.Id,
                        u.Email,
                        u.Role,
                    })
                    .OrderBy(u => u.Email)
                    .ToListAsync();

                return await pendingUserInvites
                    .ToAsyncEnumerable()
                    .Select(
                        async (pendingUserInvite, _, cancellationToken) =>
                        {
                            var userPreReleaseRoles = await userPreReleaseRoleRepository
                                .Query(ResourceRoleFilter.PendingOnly)
                                .WhereForUser(pendingUserInvite.UserId)
                                .Select(uprr => new UserPreReleaseRoleViewModel
                                {
                                    Id = uprr.Id,
                                    Publication = uprr.ReleaseVersion.Release.Publication.Title,
                                    Release = uprr.ReleaseVersion.Release.Title,
                                })
                                .ToListAsync(cancellationToken);

                            var userPublicationRoles = await userPublicationRoleRepository
                                .Query(ResourceRoleFilter.PendingOnly)
                                .WhereForUser(pendingUserInvite.UserId)
                                .Select(upr => new UserPublicationRoleViewModel
                                {
                                    Id = upr.Id,
                                    Publication = upr.Publication.Title,
                                    Role = upr.Role,
                                })
                                .ToListAsync(cancellationToken);

                            return new PendingInviteViewModel
                            {
                                Email = pendingUserInvite.Email,
                                GlobalRole = pendingUserInvite.Role!.Name!,
                                UserPublicationRoles = userPublicationRoles,
                                UserPreReleaseRoles = userPreReleaseRoles,
                            };
                        }
                    )
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, User>> InviteUser(UserInviteCreateRequest request)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => ValidateActiveUserDoesNotExist(request.Email))
            .OnSuccess(async () =>
            {
                var globalRoleToSet = request.IsBau ? Role.BauUser : Role.StandardUser;

                var createdById = userService.GetUserId();

                var user = await userRepository.CreateOrUpdate(
                    email: request.Email,
                    role: globalRoleToSet,
                    createdById: createdById,
                    createdDate: request.CreatedDate
                );

                var preReleaseRoleLatestReleaseVersionDetails = await request
                    .UserPreReleaseRoles.Select(role => role.ReleaseId)
                    .Distinct()
                    .SelectAsync(releaseId =>
                        contentDbContext
                            .ReleaseVersions.LatestReleaseVersion(releaseId)
                            .Select(rv => new { rv!.Release.PublicationId, ReleaseVersionId = rv.Id })
                            .SingleAsync()
                    );

                var publicationRolePublicationIds = request
                    .UserPublicationRoles.Select(upr => upr.PublicationId)
                    .Distinct()
                    .ToHashSet();

                // Don't create pre-release roles for releases that the user already has a publication role invite for,
                // as publication roles grant access to all releases of a publication, and are more powerful;
                var preReleaseRolesToCreate = preReleaseRoleLatestReleaseVersionDetails
                    .Where(details => !publicationRolePublicationIds.Contains(details.PublicationId))
                    .Select(details => new UserPreReleaseRoleCreateDto(
                        UserId: user.Id,
                        ReleaseVersionId: details.ReleaseVersionId,
                        CreatedById: createdById,
                        CreatedDate: request.CreatedDate ?? default
                    ))
                    .ToHashSet();

                var publicationRolesToCreate = request
                    .UserPublicationRoles.Select(userPublicationRole => new UserPublicationRoleCreateDto(
                        UserId: user.Id,
                        PublicationId: userPublicationRole.PublicationId,
                        Role: userPublicationRole.PublicationRole,
                        CreatedDate: request.CreatedDate ?? default,
                        CreatedById: createdById
                    ))
                    .ToHashSet();

                await userPreReleaseRoleRepository.CreateManyIfNotExists(preReleaseRolesToCreate);

                await userPublicationRoleRepository.CreateManyIfNotExists(publicationRolesToCreate);

                await userResourceRoleNotificationService.NotifyUserOfInvite(user.Id);

                return user;
            });
    }

    public async Task<Either<ActionResult, Unit>> CancelInvite(string email)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetPendingUserInvite(email))
            .OnSuccessVoid(async invitedUser =>
                await userRepository.SoftDeleteUser(invitedUser.Id, userService.GetUserId())
            );
    }

    public async Task<Either<ActionResult, Unit>> UpdateUserGlobalRole(Guid userId, Role targetGlobalRole)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetActiveUserById(userId))
            .OnSuccessDo(user => CheckUserGlobalRoleNeedsUpdating(user: user, targetGlobalRole: targetGlobalRole))
            .OnSuccessVoid(() => userRepository.UpdateGlobalRole(userId: userId, newRole: targetGlobalRole));
    }

    public async Task<Either<ActionResult, Unit>> DeleteUser(string email)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetActiveUserByEmail(email))
            .OnSuccessCombineWith(async _ => await GetIdentityUser(email))
            .OnSuccessVoid(async tuple =>
            {
                var (activeInternalUser, identityUser) = tuple;

                await contentDbContext.RequireTransaction(async () =>
                {
                    await userManager.DeleteAsync(identityUser);

                    await userRepository.SoftDeleteUser(activeInternalUser.Id, userService.GetUserId());
                });
            });
    }

    private static Either<ActionResult, Unit> CheckUserGlobalRoleNeedsUpdating(User user, Role targetGlobalRole)
    {
        var userGlobalRole = user.GetGlobalRole();
        var usersCurrentGlobalRoleIsBau = userGlobalRole == Role.BauUser;

        if (usersCurrentGlobalRoleIsBau && targetGlobalRole == Role.BauUser)
        {
            return ValidationUtils.ValidationActionResult(ValidationErrorMessages.UserIsAlreadyBauUser);
        }

        if (!usersCurrentGlobalRoleIsBau && targetGlobalRole == Role.StandardUser)
        {
            return ValidationUtils.ValidationActionResult(ValidationErrorMessages.UserIsAlreadyStandardUser);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, User>> GetActiveUserById(Guid userId) =>
        await userRepository.FindActiveUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, User>> GetActiveUserByEmail(string email) =>
        await userRepository.FindActiveUserByEmail(email) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, ApplicationUser>> GetIdentityUser(string email) =>
        await usersAndRolesDbContext.Users.SingleOrNotFoundAsync(user => user.Email == email);

    private async Task<Either<ActionResult, Unit>> ValidateActiveUserDoesNotExist(string email) =>
        await userRepository.FindActiveUserByEmail(email) is not null
            ? ValidationUtils.ValidationActionResult(ValidationErrorMessages.UserAlreadyExists)
            : Unit.Instance;

    private async Task<Either<ActionResult, User>> GetPendingUserInvite(string email) =>
        await userRepository.FindPendingUserInviteByEmail(email)
        ?? new Either<ActionResult, User>(
            ValidationUtils.ValidationActionResult(ValidationErrorMessages.InviteNotFound)
        );
}
