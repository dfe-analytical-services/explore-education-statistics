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
    AbstractUserResourceRoleRepository<UserReleaseRole, ReleaseVersion, ReleaseRole>(contentDbContext), 
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

    public async Task RemoveManyRolesAndInvites(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default)
    {
        var inviteKeys = userReleaseRoles
            .Select(urr => new
            {
                urr.ReleaseVersionId,
                urr.Role,
                Email = urr.User.Email.ToLower()
            })
            .ToList();

        var invites = await ContentDbContext.UserReleaseInvites
            .Where(uri => inviteKeys
                .Contains(new
                {
                    uri.ReleaseVersionId,
                    uri.Role,
                    Email = uri.Email.ToLower()
                }))
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

        var query = ContentDbContext.UserReleaseRoles
            .Where(urr => allReleaseVersionIds.Contains(urr.ReleaseVersionId));

        if (userId.HasValue)
        {
            query = query.Where(urr => urr.UserId == userId);
        }

        if (rolesToInclude.Any())
        {
            query = query.Where(urr => rolesToInclude.Contains(urr.Role));
        }

        var userReleaseRoles = await query
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

        var query = ContentDbContext.UserReleaseRoles
            .Where(urr => urr.ReleaseVersionId == releaseVersionId);

        if (userId.HasValue)
        {
            query = query.Where(urr => urr.UserId == userId);
        }

        if (rolesToInclude.Any())
        {
            query = query.Where(urr => rolesToInclude.Contains(urr.Role));
        }

        var userReleaseRoles = await query
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
            await userReleaseInviteRepository.RemoveByUser(
                email: userEmail,
                cancellationToken: cancellationToken);

            await RemoveMany(userReleaseRoles, cancellationToken);
        });
    }

    private async Task<string> GetUserEmail(Guid userId, CancellationToken cancellationToken)
    {
        return (await userRepository.FindById(userId, cancellationToken))!
            .Email;
    }
}
