#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationRoleRepository(
        ContentDbContext contentDbContext, 
        INewPermissionsSystemHelper newPermissionsSystemHelper) : 
        AbstractUserResourceRoleRepository<UserPublicationRole, Publication, PublicationRole>(contentDbContext), 
        IUserPublicationRoleRepository
    {
        // This method will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
        public async Task<UserPublicationRole?> TryCreate(Guid userId, Guid publicationId, PublicationRole publicationRole, Guid createdById)
        {
            var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
                await newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                    publicationRoleToCreate: publicationRole,
                    userId: userId,
                    publicationId: publicationId);

            if (newSystemPublicationRoleToRemove.HasValue)
            {
                var userPublicationRole = await GetUserPublicationRole(
                    userId: userId,
                    publicationId: publicationId,
                    role: newSystemPublicationRoleToRemove.Value,
                    includeNewPermissionsSystemRoles: true);

                await Remove(userPublicationRole!, createdById);
            }

            UserPublicationRole? createdNewPermissionsSystemPublicationRole = null;

            if (newSystemPublicationRoleToCreate.HasValue)
            {
                createdNewPermissionsSystemPublicationRole = await Create(
                    userId: userId,
                    resourceId: publicationId, 
                    role: newSystemPublicationRoleToCreate.Value,
                    createdById: createdById);
            }

            return publicationRole.IsNewPermissionsSystemPublicationRole()
                ? createdNewPermissionsSystemPublicationRole
                : await Create(
                    userId: userId,
                    resourceId: publicationId,
                    role: publicationRole,
                    createdById: createdById);
        }

        // This method will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
        public async Task Remove(UserPublicationRole userPublicationRole, Guid deletedById)
        {
            contentDbContext.UserPublicationRoles.Remove(userPublicationRole);
            await contentDbContext.SaveChangesAsync();

            if (userPublicationRole.Role.IsNewPermissionsSystemPublicationRole())
            {
                return;
            }

            var newSystemPublicationRoleToRemove = await newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToDelete(userPublicationRole);

            if (newSystemPublicationRoleToRemove is null)
            {
                return;
            }

            contentDbContext.UserPublicationRoles.Remove(newSystemPublicationRoleToRemove);
            await contentDbContext.SaveChangesAsync();
        }

        protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceId(
            Guid publicationId, 
            bool includeNewPermissionsSystemRoles = false)
        {
            var query = includeNewPermissionsSystemRoles
                ? ContentDbContext.AllUserPublicationRoles
                : ContentDbContext.UserPublicationRoles;

            return query
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

        // The optional parameter 'includeNewPermissionsSystemRoles' is purely here to assist with the
        // code that SYNCS the creation and removal of the NEW permissions system publication role with the
        // OLD roles. It is not ideal to have it here, as this class is abstract and is not specific to
        // publication roles. However, it was put here as a temporary parameter that will be removed
        // in EES-6196, when we no longer have to cater for the old roles. Due to it being a short-lived temporary
        // parameter, it was not worth refactoring this class and the base class.
        public Task<List<PublicationRole>> GetAllRolesByUserAndPublication(
            Guid userId, 
            Guid publicationId, 
            bool includeNewPermissionsSystemRoles = false)
        {
            return GetAllResourceRolesByUserAndResource(
                userId: userId, 
                resourceId: publicationId,
                includeNewPermissionsSystemRoles: includeNewPermissionsSystemRoles);
        }

        // The optional parameter 'includeNewPermissionsSystemRoles' is purely here to assist with the
        // code that SYNCS the creation and removal of the NEW permissions system publication role with the
        // OLD roles. It is not ideal to have it here, as this class is abstract and is not specific to
        // publication roles. However, it was put here as a temporary parameter that will be removed
        // in EES-6196, when we no longer have to cater for the old roles. Due to it being a short-lived temporary
        // parameter, it was not worth refactoring this class and the base class.
        public async Task<UserPublicationRole?> GetUserPublicationRole(
            Guid userId, 
            Guid publicationId, 
            PublicationRole role,
            bool includeNewPermissionsSystemRoles = true)
        {
            return await GetResourceRole(
                userId: userId, 
                resourceId: publicationId, 
                role: role,
                includeNewPermissionsSystemRoles: includeNewPermissionsSystemRoles);
        }

        // The optional parameter 'includeNewPermissionsSystemRoles' is purely here to assist with the
        // code that SYNCS the creation and removal of the NEW permissions system publication role with the
        // OLD roles. It is not ideal to have it here, as this class is abstract and is not specific to
        // publication roles. However, it was put here as a temporary parameter that will be removed
        // in EES-6196, when we no longer have to cater for the old roles. Due to it being a short-lived temporary
        // parameter, it was not worth refactoring this class and the base class.
        public async Task<IReadOnlyList<UserPublicationRole>> ListUserPublicationRolesByUserAndPublication(
            Guid userId, 
            Guid publicationId,
            bool includeNewPermissionsSystemRoles = false)
        {
            var query = includeNewPermissionsSystemRoles
                ? ContentDbContext.AllUserPublicationRoles
                : ContentDbContext.UserPublicationRoles;

            return await query
                .Where(urr => urr.UserId == userId)
                .Where(urr => urr.PublicationId == publicationId)
                .ToListAsync();
        }

        public Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
        {
            return UserHasRoleOnResource(userId, publicationId, role);
        }
    }
}
