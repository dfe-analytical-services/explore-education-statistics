using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserReleaseRoleRepository : IUserReleaseRoleRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserReleaseRoleRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<UserReleaseRole> GetByRole(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await _contentDbContext.UserReleaseRoles.FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.ReleaseId == releaseId &&
                r.Role == role);
        }
    }
}
