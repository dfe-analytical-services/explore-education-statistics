#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleAndInviteManager(
    ContentDbContext contentDbContext,
    IUserPublicationInviteRepository userPublicationInviteRepository,
    IUserRepository userRepository) : 
    UserResourceRoleRepositoryBase<UserPublicationRole, Publication, PublicationRole>(contentDbContext, userRepository), 
    IUserPublicationRoleAndInviteManager
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

    public async Task RemoveRoleAndInvite(
        UserPublicationRole userPublicationRole,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await GetUserEmail(userPublicationRole.UserId, cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            // Remove the associated invites if there are any
            await userPublicationInviteRepository.Remove(
                publicationId: userPublicationRole.PublicationId,
                email: userEmail,
                role: userPublicationRole.Role,
                cancellationToken: cancellationToken);

            await Remove(userPublicationRole, cancellationToken);
        });
    }

    public async Task RemoveRolesAndInvites(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default)
    {
        if (!userPublicationRoles.Any())
        {
            return;
        }

        var inviteKeys = userPublicationRoles
            .Select(upr => (upr.PublicationId, upr.Role, upr.User.Email))
            .ToHashSet();

        var invites = await ContentDbContext.UserPublicationInvites
            .Where(upi => inviteKeys.Contains(
                new ValueTuple<Guid, PublicationRole, string>(
                    upi.PublicationId,
                    upi.Role,
                    upi.Email
                )))
            .ToListAsync(cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            // Remove all of the associated invites if there are any
            await userPublicationInviteRepository.RemoveMany(invites, cancellationToken);

            await RemoveMany(userPublicationRoles, cancellationToken);
        });
    }

    public async Task RemoveAllRolesAndInvitesForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await GetUserEmail(userId, cancellationToken);

        var userPublicationRoles = await ContentDbContext.UserPublicationRoles
            .Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            await userPublicationInviteRepository.RemoveByUserEmail(
                email: userEmail,
                cancellationToken: cancellationToken);

            await RemoveMany(userPublicationRoles, cancellationToken);
        });
    }
}
