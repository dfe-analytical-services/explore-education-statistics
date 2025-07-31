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

public class UserReleaseRoleAndInviteManager(
    ContentDbContext contentDbContext,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserRepository userRepository) :
    UserResourceRoleRepositoryBase<UserReleaseRole, ReleaseVersion, ReleaseRole>(contentDbContext, userRepository), 
    IUserReleaseRoleAndInviteManager
{
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

    public async Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId)
    {
        return await GetDistinctResourceRolesByUser(userId);
    }

    public async Task<List<ReleaseRole>> GetAllRolesByUserAndRelease(Guid userId, Guid releaseVersionId)
    {
        return await GetAllResourceRolesByUserAndResource(userId, releaseVersionId);
    }

    public async Task<List<ReleaseRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
    {
        return await ContentDbContext
            .UserReleaseRoles
            .Where(role => role.UserId == userId && role.ReleaseVersion.Release.PublicationId == publicationId)
            .Select(role => role.Role)
            .Distinct()
            .ToListAsync();
    }

    public async Task<UserReleaseRole?> GetUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role)
    {
        return await GetResourceRole(userId, releaseVersionId, role);
    }

    public async Task<bool> HasUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role)
    {
        return await UserHasRoleOnResource(userId, releaseVersionId, role);
    }

    public async Task<bool> HasUserReleaseRole(string email, Guid releaseVersionId, ReleaseRole role)
    {
        return await UserHasRoleOnResource(email, releaseVersionId, role);
    }

    public async Task<List<UserReleaseRole>> ListUserReleaseRoles(Guid releaseVersionId, ReleaseRole[]? rolesToInclude)
    {
        return await ListResourceRoles(releaseVersionId, rolesToInclude);
    }

    public async Task RemoveRoleAndInvite(
        UserReleaseRole userReleaseRole,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await GetUserEmail(userReleaseRole.UserId, cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            // Remove the associated invites if there are any
            await userReleaseInviteRepository.Remove(
                releaseVersionId: userReleaseRole.ReleaseVersionId,
                email: userEmail,
                role: userReleaseRole.Role,
                cancellationToken: cancellationToken);

            await Remove(userReleaseRole, cancellationToken);
        });
    }

    public async Task RemoveRolesAndInvites(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default)
    {
        if (!userReleaseRoles.Any())
        {
            return;
        }

        var inviteKeys = userReleaseRoles
            .Select(upr => (upr.ReleaseVersionId, upr.Role, upr.User.Email))
            .ToHashSet();

        var invites = await ContentDbContext.UserReleaseInvites
            .Where(upi => inviteKeys.Contains(
                new ValueTuple<Guid, ReleaseRole, string>(
                    upi.ReleaseVersionId,
                    upi.Role,
                    upi.Email
                )))
            .ToListAsync(cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            // Remove all of the associated invites if there are any
            await userReleaseInviteRepository.RemoveMany(invites, cancellationToken);

            await RemoveMany(userReleaseRoles, cancellationToken);
        });
    }

    public async Task RemoveAllRolesAndInvitesForPublication(
        Guid publicationId,
        Guid? userId = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude)
    {
        var userEmail = userId.HasValue 
            ? await GetUserEmail(userId.Value, cancellationToken)
            : null;

        var allReleaseVersionIds = ContentDbContext.ReleaseVersions
            .Where(rv => rv.Release.PublicationId == publicationId)
            .Select(rv => rv.Id)
            .ToHashSet();

        var userReleaseRoles = await ContentDbContext.UserReleaseRoles
            .Where(urr => allReleaseVersionIds.Contains(urr.ReleaseVersionId))
            .If(userId.HasValue)
                .ThenWhere(urr => urr.UserId == userId)
            .If(rolesToInclude.Any())
                .ThenWhere(i => rolesToInclude.Contains(i.Role))
            .ToListAsync(cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            await userReleaseInviteRepository.RemoveByPublication(
                publicationId: publicationId,
                email: userEmail,
                cancellationToken: cancellationToken,
                rolesToInclude: rolesToInclude);

            await RemoveMany(userReleaseRoles, cancellationToken);
        });
    }

    public async Task RemoveAllRolesAndInvitesForReleaseVersion(
        Guid releaseVersionId,
        Guid? userId = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude)
    {
        var userEmail = userId.HasValue
            ? await GetUserEmail(userId.Value, cancellationToken)
            : null;

        var userReleaseRoles = await ContentDbContext.UserReleaseRoles
            .Where(urr => urr.ReleaseVersionId == releaseVersionId)
            .If(userId.HasValue)
                .ThenWhere(urr => urr.UserId == userId)
            .If(rolesToInclude.Any())
                .ThenWhere(i => rolesToInclude.Contains(i.Role))
            .ToListAsync(cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            await userReleaseInviteRepository.RemoveByReleaseVersion(
                releaseVersionId: releaseVersionId,
                email: userEmail,
                cancellationToken: cancellationToken,
                rolesToInclude: rolesToInclude);

            await RemoveMany(userReleaseRoles, cancellationToken);
        });
    }

    public async Task RemoveAllRolesAndInvitesForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await GetUserEmail(userId, cancellationToken);

        var userReleaseRoles = await ContentDbContext.UserReleaseRoles
            .Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await ContentDbContext.RequireTransaction(async () =>
        {
            await userReleaseInviteRepository.RemoveByUserEmail(
                email: userEmail,
                cancellationToken: cancellationToken);

            await RemoveMany(userReleaseRoles, cancellationToken);
        });
    }
}
