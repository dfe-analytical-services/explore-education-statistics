#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserRepository(ContentDbContext contentDbContext) : IUserRepository
{
    public async Task<User?> FindByEmail(string email)
    {
        return await contentDbContext.Users
            .SingleOrDefaultAsync(u =>
                u.Email.ToLower().Equals(email.ToLower())
                && u.SoftDeleted == null);
    }

    public async Task<User> FindDeletedUserPlaceholder()
    {
        // This user should be seeded in the ContentDbContext as part of the migrations, so should always exist.
        return await contentDbContext.Users
            .SingleAsync(u => u.Email.Equals(User.DeletedUserPlaceholderEmail));
    }
}
