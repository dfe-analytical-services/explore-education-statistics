#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleRepository(ContentDbContext contentDbContext, ILogger<UserReleaseRoleRepository> logger)
    : IUserReleaseRoleRepository
{
    public async Task<UserReleaseRole> Create(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var newUserReleaseRole = new UserReleaseRole
        {
            UserId = userId,
            ReleaseVersionId = releaseVersionId,
            Role = role,
            Created = createdDate?.ToUniversalTime() ?? DateTime.UtcNow,
            CreatedById = createdById,
        };

        contentDbContext.UserReleaseRoles.Add(newUserReleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserReleaseRole;
    }

    public async Task CreateManyIfNotExists(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        var newUserReleaseRoles = await userReleaseRoles
            .ToAsyncEnumerable()
            .WhereAwait(async userReleaseRole =>
                !await UserHasRoleOnReleaseVersion(
                    userId: userReleaseRole.UserId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    role: userReleaseRole.Role
                )
            )
            .ToListAsync();

        contentDbContext.UserReleaseRoles.AddRange(newUserReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserReleaseRole?> GetUserReleaseRole(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    )
    {
        return await Query()
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<UserReleaseRole> Query(
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active
    ) =>
        resourceRoleStatusFilter switch
        {
            ResourceRoleStatusFilter.Active => contentDbContext.UserReleaseRolesForActiveUsers,
            ResourceRoleStatusFilter.Pending => contentDbContext.UserReleaseRolesForPendingInvites,
            ResourceRoleStatusFilter.All => contentDbContext.UserReleaseRolesForActiveOrPending,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resourceRoleStatusFilter),
                resourceRoleStatusFilter,
                null
            ),
        };

    public async Task Remove(UserReleaseRole userReleaseRole, CancellationToken cancellationToken = default)
    {
        contentDbContext.UserReleaseRoles.Remove(userReleaseRole);

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMany(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!userReleaseRoles.Any())
        {
            return;
        }

        contentDbContext.UserReleaseRoles.RemoveRange(userReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userReleaseRoles = await contentDbContext
            .UserReleaseRoles.Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await RemoveMany(userReleaseRoles, cancellationToken);
    }

    public async Task<bool> UserHasRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleStatusFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(role)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<ReleaseRole>();

        return await Query(resourceRoleStatusFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<ReleaseRole>();

        return await Query(resourceRoleStatusFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    // The optional 'emailSent' date parameter will be removed in EES-6511 when we stop using UserReleaseRoleInvites/UserPublicationRoleInvites
    // altogether. At that point, a role will always exist at the point of sending an email; which means we can always set the date at the point
    // of sending.
    public async Task MarkEmailAsSent(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        DateTimeOffset? emailSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userReleaseRole = await GetUserReleaseRole(
            userId: userId,
            releaseVersionId: releaseVersionId,
            role: role,
            cancellationToken: cancellationToken
        );

        if (userReleaseRole is null)
        {
            throw new InvalidOperationException(
                $"No User Release Role found for {nameof(userId)} '{userId}', {nameof(releaseVersionId)} '{releaseVersionId}', {nameof(role)} '{role}'"
            );
        }

        userReleaseRole.EmailSent = emailSent?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }
}
