#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserRepository(ContentDbContext contentDbContext) : IUserRepository
{
    public async Task<User?> FindPendingUserInviteByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await contentDbContext
            .Users.WhereIsPendingInvite()
            .Where(u => u.Email.ToLower().Equals(email.ToLower()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindActiveUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await contentDbContext
            .Users.Where(u => u.Active)
            .Where(u => u.Email.ToLower().Equals(email.ToLower()))
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
        return await contentDbContext
            .Users.Where(u => !u.SoftDeleted.HasValue)
            .Where(u => u.Email.ToLower().Equals(email.ToLower()))
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

    public async Task<User> CreateOrUpdate(
        string email,
        GlobalRoles.Role role,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default
    ) =>
        await CreateOrUpdate(
            email: email,
            roleId: role.GetEnumValue(),
            createdById: createdById,
            createdDate: createdDate,
            cancellationToken: cancellationToken
        );

    public async Task<User> CreateOrUpdate(
        string email,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        if (createdDate > DateTimeOffset.UtcNow)
        {
            throw new ArgumentException($"{nameof(User)} created date cannot be a future date.");
        }

        var existingUser = await contentDbContext.Users.SingleOrDefaultAsync(i =>
            i.Email.ToLower().Equals(email.ToLower())
        );

        return existingUser is null
            ? await CreateNewUser(
                email: email,
                roleId: roleId,
                createdById: createdById,
                createdDate: createdDate,
                cancellationToken: cancellationToken
            )
            : await UpdateExistingUser(
                existingUser: existingUser,
                roleId: roleId,
                createdById: createdById,
                createdDate: createdDate,
                cancellationToken: cancellationToken
            );
    }

    public async Task SoftDeleteUser(User activeUser, Guid deletedById, CancellationToken cancellationToken = default)
    {
        contentDbContext.Attach(activeUser);

        activeUser.Active = false;
        activeUser.SoftDeleted = DateTime.UtcNow;
        activeUser.DeletedById = deletedById;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> CreateNewUser(
        string email,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken
    )
    {
        var newUser = new User
        {
            Email = email.ToLower(),
            RoleId = roleId,
            Active = false,
            CreatedById = createdById,
            Created = createdDate ?? DateTimeOffset.UtcNow,
        };

        contentDbContext.Users.Add(newUser);

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return newUser;
    }

    private async Task<User> UpdateExistingUser(
        User existingUser,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken
    )
    {
        return existingUser.Active ? throw new InvalidOperationException("Cannot update a user that is active.")
            : existingUser.SoftDeleted.HasValue
                ? await ResetSoftDeletedUser(
                    user: existingUser,
                    createdById: createdById,
                    createdDate: createdDate,
                    roleId: roleId,
                    cancellationToken: cancellationToken
                )
            : existingUser.InviteHasExpired()
                ? await ResetExpiredUserInvite(
                    user: existingUser,
                    createdById: createdById,
                    createdDate: createdDate,
                    roleId: roleId,
                    cancellationToken: cancellationToken
                )
            : await ResetPendingUserInvite(user: existingUser, roleId: roleId, cancellationToken: cancellationToken);
    }

    private async Task<User> ResetPendingUserInvite(User user, string roleId, CancellationToken cancellationToken)
    {
        var higherRoles = GlobalRoles.GetHigherRoles(
            EnumUtil.GetFromEnumValue<GlobalRoles.Role>(roleId).GetEnumLabel()
        );

        // For pending user invites, only the update role if the new one outranks (or equals) the existing one
        var newRoleId = higherRoles.Contains(EnumUtil.GetFromEnumValue<GlobalRoles.Role>(user.RoleId).GetEnumLabel())
            ? user.RoleId
            : roleId;

        user.RoleId = newRoleId;

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task<User> ResetSoftDeletedUser(
        User user,
        Guid createdById,
        DateTimeOffset? createdDate,
        string roleId,
        CancellationToken cancellationToken
    )
    {
        user.SoftDeleted = null;
        user.DeletedById = null;
        user.FirstName = null;
        user.LastName = null;
        user.CreatedById = createdById;
        user.Created = createdDate ?? DateTimeOffset.UtcNow;
        user.RoleId = roleId;

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task<User> ResetExpiredUserInvite(
        User user,
        Guid createdById,
        DateTimeOffset? createdDate,
        string roleId,
        CancellationToken cancellationToken
    )
    {
        user.CreatedById = createdById;
        user.Created = createdDate ?? DateTimeOffset.UtcNow;
        user.RoleId = roleId;

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }
}
