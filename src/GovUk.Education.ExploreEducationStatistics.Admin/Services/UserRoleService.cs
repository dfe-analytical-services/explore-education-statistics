#nullable enable
using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserRoleService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
    IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IUserService userService,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserPreReleaseRoleRepository userPreReleaseRoleRepository,
    IUserRepository userRepository,
    UserManager<ApplicationUser> identityUserManager,
    IGlobalRoleService globalRoleService
) : IUserRoleService
{
    public async Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRolesForUser(
        Guid userId
    ) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(_ => FindActiveUser(userId))
            .OnSuccess(async () =>
                await userPublicationRoleRepository
                    .Query()
                    .WhereForUser(userId)
                    .Include(upr => upr.User)
                    .Include(upr => upr.Publication)
                    .OrderBy(upr => upr.Publication.Title)
                    .Select(upr => new UserPublicationRoleViewModel
                    {
                        Id = upr.Id,
                        Publication = upr.Publication.Title,
                        Role = upr.Role,
                    })
                    .ToListAsync()
            );

    public async Task<
        Either<ActionResult, List<UserPublicationRoleWithUserViewModel>>
    > GetPublicationRolesForPublication(Guid publicationId, CancellationToken cancellationToken = default) =>
        await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async () => (
                    await userPublicationRoleRepository
                        .Query()
                        .WhereForPublication(publicationId)
                        .Include(upr => upr.User)
                        .Include(upr => upr.Publication)
                        .Select(upr => new UserPublicationRoleWithUserViewModel
                        {
                            Id = upr.Id,
                            Publication = upr.Publication.Title,
                            Role = upr.Role,
                            UserId = upr.UserId,
                            UserName = upr.User.DisplayName,
                            Email = upr.User.Email,
                        })
                        .ToListAsync(cancellationToken)
                ).OrderBy(upr => upr.UserName).ToList());

    public async Task<
        Either<ActionResult, List<UserPublicationRoleInviteViewModel>>
    > GetPublicationRoleInvitesForPublication(Guid publicationId, CancellationToken cancellationToken = default) =>
        await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async () => (
                    await userPublicationRoleRepository
                        .Query(ResourceRoleFilter.PendingOnly)
                        .WhereForPublication(publicationId)
                        .Include(upr => upr.User)
                        .Include(upr => upr.Publication)
                        .Select(upr => new UserPublicationRoleInviteViewModel
                        {
                            RoleId = upr.Id,
                            Role = upr.Role,
                            UserId = upr.UserId,
                            Email = upr.User.Email,
                        })
                        .ToListAsync(cancellationToken)
                ).OrderBy(upr => upr.Email).ToList());

    public async Task<Either<ActionResult, Unit>> AddPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role
    ) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await GetIdentityUser(userId)
                    .OnSuccessCombineWith(_ => contentPersistenceHelper.CheckEntityExists<Publication>(publicationId))
                    .OnSuccessDo(_ => ValidatePublicationRoleCanBeAdded(userId, publicationId, role))
                    .OnSuccessVoid(async tuple =>
                    {
                        var (user, publication) = tuple;

                        await contentDbContext.RequireTransaction(async () =>
                        {
                            var createdUserPublicationRole = await userPublicationRoleRepository.Create(
                                userId: userId,
                                publicationId: publication.Id,
                                role: role,
                                createdById: userService.GetUserId()
                            );

                            await globalRoleService.UpgradeToGlobalRoleIfRequired(user, RoleNames.Analyst);

                            await userResourceRoleNotificationService.NotifyUserOfNewPublicationRole(
                                createdUserPublicationRole!.Id
                            );
                        });
                    });
            });

    public async Task<Either<ActionResult, Unit>> InviteDrafter(
        string email,
        Guid publicationId,
        CancellationToken cancellationToken = default
    ) =>
        await contentDbContext
            .Publications.SingleOrNotFoundAsync(p => p.Id == publicationId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateDrafters)
            .OnSuccessDo(_ => ValidateDrafterRoleCanBeAdded(email, publicationId))
            .OnSuccessVoid(async _ =>
                await AddDrafterRole(email: email, publicationId: publicationId, cancellationToken: cancellationToken)
            );

    public async Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid userPublicationRoleId) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => FindUserPublicationRole(userPublicationRoleId))
            .OnSuccess(RemoveUserPublicationRole);

    public async Task<Either<ActionResult, Unit>> RemoveDrafter(Guid userPublicationRoleId) =>
        await FindUserDrafterRole(userPublicationRoleId)
            .OnSuccessDo(async userPublicationRole =>
                await userService.CheckCanUpdateDrafters(userPublicationRole.Publication)
            )
            .OnSuccess(RemoveUserPublicationRole);

    public async Task<Either<ActionResult, Unit>> RemoveAllUserResourceRoles(Guid userId) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async _ =>
            {
                return await FindActiveUser(userId)
                    .OnSuccess(async _ =>
                    {
                        await userPreReleaseRoleRepository.RemoveForUser(userId);
                        await userPublicationRoleRepository.RemoveForUser(userId);

                        await usersAndRolesPersistenceHelper
                            .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                            .OnSuccessDo(async user =>
                            {
                                var existingRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

                                await identityUserManager.RemoveFromRolesAsync(user, existingRoleNames);
                            });

                        return Unit.Instance;
                    });
            });

    private async Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(UserPublicationRole userPublicationRole)
    {
        var removed = await userPublicationRoleRepository.RemoveById(userPublicationRole.Id);

        if (!removed)
        {
            throw new InvalidOperationException(
                $"Failed to remove User Publication Role with ID {userPublicationRole.Id}"
            );
        }

        var checkAspNetUserExistsResult = await GetIdentityUser(userPublicationRole.UserId);

        if (checkAspNetUserExistsResult.IsRight)
        {
            await globalRoleService.DowngradeFromGlobalRoleIfRequired(
                checkAspNetUserExistsResult.Right,
                RoleNames.Analyst
            );
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> ValidatePublicationRoleCanBeAdded(
        Guid userId,
        Guid publicationId,
        PublicationRole role
    )
    {
        if (role is PublicationRole.Drafter)
        {
            return await ValidateDrafterRoleCanBeAdded(userId: userId, publicationId: publicationId);
        }

        if (
            await userPublicationRoleRepository.UserHasRoleOnPublication(
                userId: userId,
                publicationId: publicationId,
                role: role
            )
        )
        {
            return ValidationActionResult(UserAlreadyHasResourceRole);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> ValidateDrafterRoleCanBeAdded(string email, Guid publicationId)
    {
        var user = await userRepository.FindUserByEmail(email);

        if (user is null)
        {
            return Unit.Instance;
        }

        return await ValidateDrafterRoleCanBeAdded(userId: user.Id, publicationId: publicationId);
    }

    private async Task<Either<ActionResult, Unit>> ValidateDrafterRoleCanBeAdded(Guid userId, Guid publicationId)
    {
        var userPublicationRoles = (
            await userPublicationRoleRepository
                .Query(ResourceRoleFilter.All)
                .WhereForUser(userId)
                .WhereForPublication(publicationId)
                .Select(upr => upr.Role)
                .ToListAsync()
        ).ToHashSet();

        if (userPublicationRoles.Contains(PublicationRole.Approver))
        {
            return ValidationActionResult(UserAlreadyHasMorePowerfulRole);
        }

        if (userPublicationRoles.Contains(PublicationRole.Drafter))
        {
            return ValidationActionResult(UserAlreadyHasResourceRole);
        }

        return Unit.Instance;
    }

    private async Task AddDrafterRole(string email, Guid publicationId, CancellationToken cancellationToken)
    {
        var activeUser = await userRepository.FindActiveUserByEmail(email, cancellationToken);

        await contentDbContext.RequireTransaction(async () =>
        {
            var user =
                activeUser
                ?? await userRepository.CreateOrUpdate(
                    email: email,
                    role: Role.Analyst,
                    createdById: userService.GetUserId(),
                    cancellationToken: cancellationToken
                );

            var createdUserDrafterRole = await userPublicationRoleRepository.Create(
                userId: user.Id,
                publicationId: publicationId,
                role: PublicationRole.Drafter,
                createdById: userService.GetUserId(),
                cancellationToken: cancellationToken
            );

            if (createdUserDrafterRole is null)
            {
                throw new UnreachableException("Unexpected error. Failed to create drafter role.");
            }

            await userResourceRoleNotificationService.NotifyUserOfNewDrafterRole(
                userPublicationRoleId: createdUserDrafterRole.Id,
                cancellationToken: cancellationToken
            );

            if (user.Active)
            {
                var identityUser = await GetIdentityUser(user.Id, cancellationToken);

                await globalRoleService.UpgradeToGlobalRoleIfRequired(identityUser.Right, RoleNames.Analyst);
            }
        });
    }

    private async Task<Either<ActionResult, User>> FindActiveUser(Guid userId) =>
        await userRepository.FindActiveUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, UserPublicationRole>> FindUserPublicationRole(
        Guid userPublicationRoleId,
        bool hydrateWithPublication = false
    ) =>
        await userPublicationRoleRepository.GetById(userPublicationRoleId)
        ?? new Either<ActionResult, UserPublicationRole>(new NotFoundResult());

    private async Task<Either<ActionResult, UserPublicationRole>> FindUserDrafterRole(Guid userPublicationRoleId) =>
        await userPublicationRoleRepository
            .Query(ResourceRoleFilter.All)
            .Where(upr => upr.Id == userPublicationRoleId)
            .WhereRolesIn(PublicationRole.Drafter)
            .Include(upr => upr.Publication)
            .SingleOrNotFoundAsync();

    private async Task<Either<ActionResult, ApplicationUser>> GetIdentityUser(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => await usersAndRolesDbContext.Users.SingleOrNotFoundAsync(u => u.Id == userId.ToString(), cancellationToken);
}
