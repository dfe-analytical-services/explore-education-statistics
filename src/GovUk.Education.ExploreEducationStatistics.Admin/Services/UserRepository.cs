#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserRepository(
    ContentDbContext contentDbContext) : IUserRepository
{
    public async Task<User?> FindActiveUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await contentDbContext.Users
            .Where(u => u.Email.ToLower().Equals(email.ToLower()))
            .Where(u => u.Active)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await contentDbContext.Users
            .Where(u => u.Email.ToLower().Equals(email.ToLower()))
            .Where(u => u.SoftDeleted == null)
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
        Role role,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default)
        => await CreateOrUpdate(
            email: email,
            roleId: role.GetEnumValue(),
            createdById: createdById,
            createdDate: createdDate,
            cancellationToken: cancellationToken);

    public async Task<User> CreateOrUpdate(
        string email,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate = null,
        CancellationToken cancellationToken = default)
    {
        if (createdDate > DateTimeOffset.UtcNow)
        {
            throw new ArgumentException($"{nameof(User)} created date cannot be a future date.");
        }

        var existingUser = await contentDbContext
            .Users
            .SingleOrDefaultAsync(i => i.Email.ToLower().Equals(email.ToLower()));

        if (existingUser is null)
        {
            return await CreateNewUser(
                email: email,
                roleId: roleId,
                createdById: createdById,
                createdDate: createdDate,
                cancellationToken: cancellationToken);
        }

        return existingUser.Active
            ? throw new InvalidOperationException("Cannot create or update a user that is active.")
            : await UpdateExistingUser(
                existingUser: existingUser,
                roleId: roleId,
                createdById: createdById,
                createdDate: createdDate,
                cancellationToken: cancellationToken);
    }

    public async Task SoftDeleteUser(
        User activeUser,
        Guid deletedById,
        CancellationToken cancellationToken = default)
    {
        contentDbContext.Attach(activeUser);

        activeUser.Active = false;
        activeUser.SoftDeleted = DateTime.UtcNow;
        activeUser.DeletedById = deletedById;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> UpdateExistingUser(
        User existingUser,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken)
    {
        existingUser.RoleId = roleId;
        existingUser.CreatedById = createdById;
        existingUser.Created = createdDate ?? DateTimeOffset.UtcNow;
        existingUser.SoftDeleted = null;
        existingUser.DeletedById = null;
        existingUser.FirstName = null;
        existingUser.LastName = null;

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return existingUser;
    }

    private async Task<User> CreateNewUser(
        string email,
        string roleId,
        Guid createdById,
        DateTimeOffset? createdDate,
        CancellationToken cancellationToken)
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
}
