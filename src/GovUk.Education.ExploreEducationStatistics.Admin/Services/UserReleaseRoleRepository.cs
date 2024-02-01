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
    public class UserReleaseRoleRepository :
        AbstractUserResourceRoleRepository<UserReleaseRole, Release, ReleaseRole>, IUserReleaseRoleRepository
    {
        public UserReleaseRoleRepository(ContentDbContext contentDbContext) : base(contentDbContext)
        {
        }

        protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceId(Guid releaseId)
        {
            return ContentDbContext
                .UserReleaseRoles
                .Where(role => role.ReleaseId == releaseId);
        }

        protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceIds(List<Guid> releaseIds)
        {
            return ContentDbContext
                .UserReleaseRoles
                .Where(role => releaseIds.Contains(role.ReleaseId));
        }

        public async Task RemoveAllForPublication(Guid userId, Publication publication, ReleaseRole role,
            Guid deletedById)
        {
            ContentDbContext.Update(publication);
            await ContentDbContext
                .Entry(publication)
                .Collection(p => p.Releases)
                .LoadAsync();
            var allReleaseIds = publication
                .Releases // Remove on previous release versions as well
                .Select(r => r.Id)
                .ToList();
            var userReleaseRoles = await ContentDbContext.UserReleaseRoles
                .AsQueryable()
                .Where(urr =>
                    urr.UserId == userId
                    && allReleaseIds.Contains(urr.ReleaseId)
                    && urr.Role == role)
                .ToListAsync();
            await RemoveMany(userReleaseRoles, deletedById);
        }

        public Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId)
        {
            return GetDistinctResourceRolesByUser(userId);
        }

        public Task<List<ReleaseRole>> GetAllRolesByUserAndRelease(Guid userId, Guid releaseId)
        {
            return GetAllResourceRolesByUserAndResource(userId, releaseId);
        }

        public Task<List<ReleaseRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
        {
            return ContentDbContext
                .UserReleaseRoles
                .Where(role => role.UserId == userId && role.Release.PublicationId == publicationId)
                .Select(role => role.Role)
                .Distinct()
                .ToListAsync();
        }

        public async Task<UserReleaseRole?> GetUserReleaseRole(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await GetResourceRole(userId, releaseId, role);
        }

        public Task<bool> HasUserReleaseRole(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return UserHasRoleOnResource(userId, releaseId, role);
        }

        public Task<bool> HasUserReleaseRole(string email, Guid releaseId, ReleaseRole role)
        {
            return UserHasRoleOnResource(email, releaseId, role);
        }

        public Task<List<UserReleaseRole>> ListUserReleaseRoles(Guid releaseId, ReleaseRole[]? rolesToInclude)
        {
            return ListResourceRoles(releaseId, rolesToInclude);
        }
    }
}
