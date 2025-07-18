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
    AbstractUserResourceRoleRepository<UserPublicationRole, Publication, PublicationRole>(contentDbContext), 
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

    public async Task RemoveManyRolesAndInvites(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default)
    {
        var inviteKeys = userPublicationRoles
            .Select(upr => new
            {
                upr.PublicationId,
                upr.Role,
                Email = upr.User.Email.ToLower()
            })
            .ToList();

        var invites = await ContentDbContext.UserPublicationInvites
            .Where(upi => inviteKeys
                .Contains(new
                {
                    upi.PublicationId,
                    upi.Role,
                    Email = upi.Email.ToLower()
                }))
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
            await userPublicationInviteRepository.RemoveByUser(
                email: userEmail,
                cancellationToken: cancellationToken);

            await RemoveMany(userPublicationRoles, cancellationToken);
        });
    }

    private async Task<string> GetUserEmail(Guid userId, CancellationToken cancellationToken)
    {
        return (await userRepository.FindById(userId, cancellationToken))!
            .Email;
    }
}
