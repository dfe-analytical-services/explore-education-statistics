#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleRepository(ContentDbContext contentDbContext) :
    UserResourceRoleRepositoryBase<UserPublicationRoleRepository, UserPublicationRole, Publication, PublicationRole>(contentDbContext),
    IUserPublicationRoleRepository
{
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

    public async Task<UserPublicationRole?> GetUserPublicationRole(Guid userId, Guid publicationId, PublicationRole role)
    {
        return await GetResourceRole(userId, publicationId, role);
    }

    public async Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
    {
        return await UserHasRoleOnResource(userId, publicationId, role);
    }

    public new async Task Remove(
        UserPublicationRole userPublicationRole,
        CancellationToken cancellationToken = default)
    {
        await base.Remove(userPublicationRole, cancellationToken);
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
