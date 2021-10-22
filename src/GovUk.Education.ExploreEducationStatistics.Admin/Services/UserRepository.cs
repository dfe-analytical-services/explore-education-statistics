#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<User?> FindByEmail(string email)
        {
            return await _contentDbContext.Users
                .AsQueryable()
                .SingleOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));
        }
    }
}
