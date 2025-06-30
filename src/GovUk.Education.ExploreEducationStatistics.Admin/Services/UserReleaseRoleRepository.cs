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
    public class UserReleaseRoleRepository(
        ContentDbContext contentDbContext, 
        INewPermissionsSystemHelper newPermissionsSystemHelper, 
        IUserPublicationRoleRepository userPublicationRoleRepository) :
        AbstractUserResourceRoleRepository<UserReleaseRole, ReleaseVersion, ReleaseRole>(contentDbContext), IUserReleaseRoleRepository
    {
        // This class will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
        public new async Task<UserReleaseRole> Create(Guid userId, Guid releaseVersionId, ReleaseRole releaseRole, Guid createdById)
        {
            var publicationId = await ContentDbContext
                .ReleaseVersions
                .Where(r => r.Id == releaseVersionId)
                .Select(r => r.Release.PublicationId)
                .SingleAsync();

            var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) = 
                await newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                    releaseRoleToCreate: releaseRole,
                    userId: userId,
                    publicationId: publicationId);

            if (newSystemPublicationRoleToRemove.HasValue)
            {
                var userPublicationRole = await userPublicationRoleRepository.GetUserPublicationRole(
                    userId: userId,
                    publicationId: publicationId,
                    role: newSystemPublicationRoleToRemove.Value);

                await userPublicationRoleRepository.Remove(userPublicationRole!, createdById);
            }

            if (newSystemPublicationRoleToCreate.HasValue)
            {
                await userPublicationRoleRepository.TryCreate(
                    userId: userId,
                    publicationId: publicationId,
                    publicationRole: newSystemPublicationRoleToCreate.Value,
                    createdById: createdById);
            }

            return await Create(
                userId: userId,
                resourceId: releaseVersionId,
                role: releaseRole,
                createdById: createdById);
        }

        // This class will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
        public async Task Remove(UserReleaseRole userReleaseRole, Guid deletedById)
        {
            await SoftDeleteUserReleaseRole(
                userReleaseRole: userReleaseRole,
                deletedById: deletedById);

            var newSystemPublicationRoleToRemove = await newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToDelete(userReleaseRole);

            if (newSystemPublicationRoleToRemove is null)
            {
                return;
            }

            await userPublicationRoleRepository.Remove(
                userPublicationRole: newSystemPublicationRoleToRemove,
                deletedById: deletedById);
        }

        protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceId(Guid releaseVersionId)
        {
            return ContentDbContext
                .UserReleaseRoles
                .Where(role => role.ReleaseVersionId == releaseVersionId);
        }

        protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceIds(List<Guid> releaseVersionIds)
        {
            return ContentDbContext
                .UserReleaseRoles
                .Where(role => releaseVersionIds.Contains(role.ReleaseVersionId));
        }

        public async Task RemoveAllForPublication(Guid userId, Publication publication, ReleaseRole role,
            Guid deletedById)
        {
            ContentDbContext.Update(publication);
            await ContentDbContext
                .Entry(publication)
                .Collection(p => p.ReleaseVersions)
                .LoadAsync();
            var allReleaseVersionIds = publication
                .ReleaseVersions // Remove on previous release versions as well
                .Select(rv => rv.Id)
                .ToList();
            var userReleaseRoles = await ContentDbContext.UserReleaseRoles
                .AsQueryable()
                .Where(urr =>
                    urr.UserId == userId
                    && allReleaseVersionIds.Contains(urr.ReleaseVersionId)
                    && urr.Role == role)
                .ToListAsync();
            await RemoveMany(userReleaseRoles, deletedById);
        }

        public Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId)
        {
            return GetDistinctResourceRolesByUser(userId);
        }

        public Task<List<ReleaseRole>> GetAllRolesByUserAndRelease(Guid userId, Guid releaseVersionId)
        {
            return GetAllResourceRolesByUserAndResource(userId, releaseVersionId);
        }

        public Task<List<ReleaseRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
        {
            return ContentDbContext
                .UserReleaseRoles
                .Where(role => role.UserId == userId && role.ReleaseVersion.PublicationId == publicationId)
                .Select(role => role.Role)
                .Distinct()
                .ToListAsync();
        }

        public async Task<UserReleaseRole?> GetUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role)
        {
            return await GetResourceRole(userId, releaseVersionId, role);
        }

        public Task<bool> HasUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role)
        {
            return UserHasRoleOnResource(userId, releaseVersionId, role);
        }

        public Task<bool> HasUserReleaseRole(string email, Guid releaseVersionId, ReleaseRole role)
        {
            return UserHasRoleOnResource(email, releaseVersionId, role);
        }

        public Task<List<UserReleaseRole>> ListUserReleaseRoles(Guid releaseVersionId, ReleaseRole[]? rolesToInclude)
        {
            return ListResourceRoles(releaseVersionId, rolesToInclude);
        }

        public async Task<IReadOnlyList<UserReleaseRole>> ListUserReleaseRolesByUserAndPublication(Guid userId, Guid publicationId)
        {
            return await ContentDbContext
                .UserReleaseRoles
                .Where(urr => urr.UserId == userId)
                .Where(urr => urr.ReleaseVersion.Release.PublicationId == publicationId)
                .ToListAsync();
        }

        private async Task SoftDeleteUserReleaseRole(UserReleaseRole userReleaseRole, Guid deletedById)
        {
            userReleaseRole.Deleted = DateTime.UtcNow;
            userReleaseRole.DeletedById = deletedById;
            ContentDbContext.Update(userReleaseRole);
            await ContentDbContext.SaveChangesAsync();
        }
    }
}
