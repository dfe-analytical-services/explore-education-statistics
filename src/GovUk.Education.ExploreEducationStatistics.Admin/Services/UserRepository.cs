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
    ContentDbContext contentDbContext, 
    UsersAndRolesDbContext usersAndRolesDbContext) : IUserRepository
{
    public async Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await contentDbContext.Users.SingleOrDefaultAsync(
            u => u.Email.ToLower().Equals(email.ToLower()) && u.SoftDeleted == null,
            cancellationToken
        );
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
        DateTime? createdDate = null)
        => await CreateOrUpdate(email, role.GetEnumValue(), createdById, createdDate);

    public async Task<User> CreateOrUpdate(
        string email,
        string roleId,
        Guid createdById,
        DateTime? createdDate = null)
    {
        if (await IdentityUserAlreadyExists(email))
        {
            throw new ArgumentException($"{nameof(User)} with email {email} already exists.");
        }

        if (createdDate > DateTime.UtcNow)
        {
            throw new ArgumentException($"{nameof(User)} created date cannot be a future date.");
        }

        var user = await contentDbContext
            .Users
            .SingleOrDefaultAsync(i => i.Email.ToLower().Equals(email.ToLower()));

        if (user != null)
        {
            user.RoleId = roleId;
            user.CreatedById = createdById;
            user.Created = createdDate ?? DateTime.UtcNow;
            user.SoftDeleted = null;
            user.DeletedById = null;
        }
        else
        {
            user = new User
            {
                Email = email.ToLower(),
                RoleId = roleId,
                Active = false,
                CreatedById = createdById,
                Created = createdDate ?? DateTime.UtcNow,
            };

            contentDbContext.Users.Add(user);
        }

        await contentDbContext.SaveChangesAsync();
        return user;
    }

    private async Task<bool> IdentityUserAlreadyExists(string email)
        => await usersAndRolesDbContext.Users
            .AnyAsync(u => u.Email!.ToLower().Equals(email.ToLower()));
}
