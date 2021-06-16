using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<UserReleaseRole> Create(Guid userId, Guid releaseId, ReleaseRole role)
        {
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                ReleaseId = releaseId,
                Role = role
            };

            var created = (await _contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
        }

        public async Task<List<ReleaseRole>> GetAllRolesByUser(Guid userId, Guid releaseId)
        {
            return await _contentDbContext.UserReleaseRoles.Where(r =>
                    r.UserId == userId &&
                    r.ReleaseId == releaseId)
                .Select(r => r.Role)
                .ToListAsync();
        }

        public async Task<UserReleaseRole> GetByUserAndRole(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await _contentDbContext.UserReleaseRoles.FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.ReleaseId == releaseId &&
                r.Role == role);
        }
    }
}
