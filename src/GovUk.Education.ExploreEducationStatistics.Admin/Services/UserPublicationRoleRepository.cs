#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationRoleRepository(
        ContentDbContext contentDbContext, 
        INewPermissionsSystemHelper newPermissionsSystemHelper) : 
        AbstractUserResourceRoleRepository<UserPublicationRole, Publication, PublicationRole>(contentDbContext), 
        IUserPublicationRoleRepository
    {
        public async Task<UserPublicationRole?> TryCreate(Guid userId, Guid publicationId, PublicationRole publicationRole, Guid createdById)
        {
            var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
                await newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(publicationRole, userId, publicationId);

            if (newSystemPublicationRoleToRemove.HasValue)
            {
                var userPublicationRole = await GetUserPublicationRole(
                    userId: userId,
                    publicationId: publicationId,
                    role: newSystemPublicationRoleToRemove.Value);

                await Remove(userPublicationRole!, createdById);
            }

            UserPublicationRole? createdNewPermissionsSystemPublicationRole = null;

            if (newSystemPublicationRoleToCreate.HasValue)
            {
                createdNewPermissionsSystemPublicationRole = await Create(
                    userId: userId,
                    resourceId: publicationId, 
                    role: newSystemPublicationRoleToCreate.Value,
                    createdById:createdById);
            }

            return publicationRole.IsNewPermissionsSystemPublicationRole()
                ? createdNewPermissionsSystemPublicationRole
                : await Create(
                    userId: userId,
                    resourceId: publicationId,
                    role: publicationRole,
                    createdById: createdById);
        }

        protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceId(Guid publicationId)
        {
            return ContentDbContext
                .UserPublicationRoles
                .Where(role => role.PublicationId == publicationId);
        }

        protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceIds(List<Guid> publicationIds)
        {
            return ContentDbContext
                .UserPublicationRoles
                .Where(role => publicationIds.Contains(role.PublicationId));
        }

        public Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId)
        {
            return GetDistinctResourceRolesByUser(userId);
        }

        public Task<List<PublicationRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
        {
            return GetAllResourceRolesByUserAndResource(userId, publicationId);
        }

        public async Task<UserPublicationRole?> GetUserPublicationRole(Guid userId, Guid publicationId, PublicationRole role)
        {
            return await GetResourceRole(userId, publicationId, role);
        }

        public Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
        {
            return UserHasRoleOnResource(userId, publicationId, role);
        }
    }
}
