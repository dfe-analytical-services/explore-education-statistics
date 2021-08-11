#nullable enable
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

        public async Task<bool> UserHasRoleOnRelease(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await _contentDbContext.UserReleaseRoles.AnyAsync(r =>
                r.UserId == userId &&
                r.ReleaseId == releaseId &&
                r.Role == role);
        }

        public async Task<bool> UserHasAnyOfRolesOnLatestRelease(Guid userId,
            Guid publicationId,
            IEnumerable<ReleaseRole> roles)
        {
            var publication = await _contentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            var latestRelease = publication.LatestRelease(checkIfLive: false);

            // Publication may have no releases
            if (latestRelease == null)
            {
                return false;
            }

            return await _contentDbContext.UserReleaseRoles
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ReleaseId == latestRelease.Id &&
                    roles.Contains(r.Role));
        }
    }
}
