#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserRepository(
    ContentDbContext contentDbContext,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserPreReleaseRoleRepository userPreReleaseRoleRepository,
    IGlobalRoleService globalRoleService
) : IUserRepository
{
    public async Task<User?> FindPendingUserInviteByEmail(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormaliseEmail(email);

        return await contentDbContext
            .Users.WhereInvitePending()
            .Where(u => u.Email.ToLower().Equals(normalizedEmail))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindActiveUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormaliseEmail(email);

        return await contentDbContext
            .Users.Where(u => u.Active)
            .Where(u => u.Email.ToLower().Equals(normalizedEmail))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindActiveUserById(Guid userId, CancellationToken cancellationToken = default)
    {
        return await contentDbContext
            .Users.Where(u => u.Active)
            .Where(u => u.Id == userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormaliseEmail(email);

        return await contentDbContext
            .Users.Where(u => !u.SoftDeleted.HasValue)
            .Where(u => u.Email.ToLower().Equals(normalizedEmail))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindUserById(Guid userId, CancellationToken cancellationToken = default)
    {
        return await contentDbContext
            .Users.Where(u => !u.SoftDeleted.HasValue)
            .Where(u => u.Id == userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User> FindDeletedUserPlaceholder(CancellationToken cancellationToken = default)
    {
        // This user should be seeded in the ContentDbContext as part of the migrations, so should always exist.
        return await contentDbContext.Users.SingleAsync(
            u => u.Email.Equals(User.DeletedUserPlaceholderEmail),
            cancellationToken
        );
    }

    public async Task<User> UpdateGlobalRole(
        Guid userId,
        GlobalRoles.Role newRole,
        CancellationToken cancellationToken = default
    )
    {
        var activeUser =
            await FindActiveUserById(userId, cancellationToken)
            ?? throw new InvalidOperationException("Cannot update the global role for a user that is not active.");

        await contentDbContext.RequireTransaction(async () =>
        {
            activeUser.RoleId = newRole.GetEnumValue();

            await contentDbContext.SaveChangesAsync(cancellationToken);

            await globalRoleService.UpdateGlobalRoleForUser(userId: userId, newRole: newRole);
        });

        return activeUser;
    }

    public async Task<User> CreateOrUpdate(
        string email,
        Guid createdById,
        GlobalRoles.Role role = GlobalRoles.Role.StandardUser,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = NormaliseEmail(email);

        if (createdDate > DateTimeOffset.UtcNow)
        {
            throw new ArgumentException($"{nameof(User)} created date cannot be a future date.");
        }

        var existingUser = await contentDbContext.Users.SingleOrDefaultAsync(
            i => i.Email.ToLower().Equals(normalizedEmail),
            cancellationToken: cancellationToken
        );

        return existingUser is null
            ? await CreateNewUser(
                email: normalizedEmail,
                role: role,
                createdById: createdById,
                createdDate: createdDate,
                cancellationToken: cancellationToken
            )
            : await UpdateExistingUser(
                existingUser: existingUser,
                role: role,
                createdById: createdById,
                createdDate: createdDate,
                cancellationToken: cancellationToken
            );
    }

    public async Task SoftDeleteUser(Guid userId, Guid deletedById, CancellationToken cancellationToken = default)
    {
        var activeUser =
            await FindUserById(userId, cancellationToken)
            ?? throw new InvalidOperationException(
                "Cannot soft delete a user that is already soft deleted, or does not exist."
            );

        await contentDbContext.RequireTransaction(async () =>
        {
            await userPreReleaseRoleRepository.RemoveForUser(userId, cancellationToken);
            await userPublicationRoleRepository.RemoveForUser(userId, cancellationToken);

            activeUser.Active = false;
            activeUser.SoftDeleted = DateTime.UtcNow;
            activeUser.DeletedById = deletedById;

            await contentDbContext.SaveChangesAsync(cancellationToken);
        });
    }

    private async Task<User> CreateNewUser(
        string email,
        GlobalRoles.Role role,
        Guid createdById,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken
    )
    {
        var normalizedEmail = NormaliseEmail(email);

        var newUser = new User
        {
            Email = normalizedEmail,
            RoleId = role.GetEnumValue(),
            Active = false,
            CreatedById = createdById,
            Created = ToUniversalTime(createdDate),
        };

        contentDbContext.Users.Add(newUser);

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return newUser;
    }

    private async Task<User> UpdateExistingUser(
        User existingUser,
        GlobalRoles.Role role,
        Guid createdById,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken
    )
    {
        if (existingUser.Active)
        {
            throw new InvalidOperationException("Cannot update a user that is active.");
        }

        return await contentDbContext.RequireTransaction(async () =>
        {
            await userPreReleaseRoleRepository.RemoveForUser(existingUser.Id);
            await userPublicationRoleRepository.RemoveForUser(existingUser.Id);

            return existingUser.SoftDeleted.HasValue
                    ? await ResetSoftDeletedUser(
                        user: existingUser,
                        createdById: createdById,
                        createdDate: createdDate,
                        role: role,
                        cancellationToken: cancellationToken
                    )
                : existingUser.IsInviteExpired()
                    ? await ResetExpiredUserInvite(
                        user: existingUser,
                        createdById: createdById,
                        createdDate: createdDate,
                        role: role,
                        cancellationToken: cancellationToken
                    )
                : await ResetPendingUserInvite(
                    user: existingUser,
                    role: role,
                    createdDate: createdDate,
                    cancellationToken: cancellationToken
                );
        });
    }

    private async Task<User> ResetSoftDeletedUser(
        User user,
        Guid createdById,
        DateTimeOffset? createdDate,
        GlobalRoles.Role role,
        CancellationToken cancellationToken
    )
    {
        user.SoftDeleted = null;
        user.DeletedById = null;
        user.FirstName = null;
        user.LastName = null;
        user.CreatedById = createdById;
        user.Created = ToUniversalTime(createdDate);
        user.RoleId = role.GetEnumValue();

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task<User> ResetExpiredUserInvite(
        User user,
        Guid createdById,
        DateTimeOffset? createdDate,
        GlobalRoles.Role role,
        CancellationToken cancellationToken
    )
    {
        user.CreatedById = createdById;
        user.Created = ToUniversalTime(createdDate);
        user.RoleId = role.GetEnumValue();

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task<User> ResetPendingUserInvite(
        User user,
        GlobalRoles.Role role,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken
    )
    {
        user.RoleId = role.GetEnumValue();
        // Always update the created date to the new one, to reset the invite expiry
        user.Created = ToUniversalTime(createdDate);

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static string NormaliseEmail(string email) => email.Trim().ToLower();

    private static DateTimeOffset ToUniversalTime(DateTimeOffset? createdDate) =>
        createdDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
}
