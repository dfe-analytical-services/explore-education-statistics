#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

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

        public async Task<bool> IsUserApproverOnLatestRelease(Guid userId, Guid publicationId)
        {
            return await UserHasAnyOfRolesOnLatestRelease(
                userId,
                publicationId,
                ListOf(ReleaseRole.Approver));
        }

        public async Task<bool> IsUserEditorOrApproverOnLatestRelease(Guid userId, Guid publicationId)
        {
            return await UserHasAnyOfRolesOnLatestRelease(
                userId,
                publicationId,
                ReleaseEditorAndApproverRoles);
        }

        public async Task<bool> IsUserPrereleaseViewerOnLatestPreReleaseRelease(Guid userId, Guid publicationId)
        {
            var publication = await ContentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            var latestRelease = publication.LatestRelease();

            // Publication may have no releases
            if (latestRelease == null
                // Release should be in prerelease
                || latestRelease.Published != null
                || latestRelease.ApprovalStatus != ReleaseApprovalStatus.Approved)
            {
                return false;
            }

            return await HasUserReleaseRole(
                userId,
                latestRelease.Id,
                ReleaseRole.PrereleaseViewer);
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

        public async Task<bool> UserHasAnyOfRolesOnLatestRelease(Guid userId,
            Guid publicationId,
            IEnumerable<ReleaseRole> roles)
        {
            var publication = await ContentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            var latestRelease = publication.LatestRelease();

            // Publication may have no releases
            if (latestRelease == null)
            {
                return false;
            }

            return await ContentDbContext.UserReleaseRoles
                .AsQueryable()
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ReleaseId == latestRelease.Id &&
                    roles.Contains(r.Role));
        }
    }
}