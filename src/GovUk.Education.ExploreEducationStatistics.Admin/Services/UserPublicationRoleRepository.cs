#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleRepository(
    ContentDbContext contentDbContext,
    INewPermissionsSystemHelper newPermissionsSystemHelper) :
    UserResourceRoleRepositoryBase<UserPublicationRoleRepository, UserPublicationRole, Publication, PublicationRole>(
        contentDbContext),
    IUserPublicationRoleRepository
{
    // This class will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for
    // the old roles.
    public async Task<UserPublicationRole?> TryCreate(
        Guid userId, 
        Guid publicationId, 
        PublicationRole publicationRole, 
        Guid createdById)
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
                role: newSystemPublicationRoleToRemove.Value);

            await Remove(userPublicationRole!);
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

    public async Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId)
    {
        return await GetDistinctResourceRolesByUser(userId);
    }

    public async Task<List<PublicationRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
    {
        return await GetAllResourceRolesByUserAndResource(userId, publicationId);
    }
    
    public async Task<IReadOnlyList<UserPublicationRole>> ListUserPublicationRolesByUserAndPublication(Guid userId, Guid publicationId)
    {
        return await ContentDbContext
            .UserPublicationRoles
            .Where(urr => urr.UserId == userId)
            .Where(urr => urr.PublicationId == publicationId)
            .ToListAsync();
    }

    public async Task<UserPublicationRole?> GetUserPublicationRole(Guid userId, Guid publicationId, PublicationRole role)
    {
        return await GetResourceRole(userId, publicationId, role);
    }

    public async Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
    {
        return await UserHasRoleOnResource(userId, publicationId, role);
    }
    
    // This class will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
    public new async Task Remove(
        UserPublicationRole userPublicationRole,
        CancellationToken cancellationToken = default)
    {
        await base.Remove(userPublicationRole, cancellationToken);

        if (userPublicationRole.Role.IsNewPermissionsSystemPublicationRole())
        {
            return;
        }

        var newSystemPublicationRoleToRemove = await newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToDelete(userPublicationRole);

        if (newSystemPublicationRoleToRemove is null)
        {
            return;
        }

        await base.Remove(newSystemPublicationRoleToRemove, cancellationToken);
    }

    public new async Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default)
    {
        await base.RemoveMany(userPublicationRoles, cancellationToken);
    }

    public async Task RemoveForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userPublicationRoles = await ContentDbContext.UserPublicationRoles
            .Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await base.RemoveMany(userPublicationRoles, cancellationToken);
    }
}
