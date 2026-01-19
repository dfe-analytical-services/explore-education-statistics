#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleRepository(ContentDbContext contentDbContext) : IUserReleaseRoleRepository
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

    public async Task<List<UserReleaseRole>> CreateManyIfNotExists(
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
                    role: userReleaseRole.Role,
                    resourceRoleFilter: ResourceRoleFilter.All,
                    cancellationToken: cancellationToken
                )
            )
            .ToListAsync(cancellationToken);

        contentDbContext.UserReleaseRoles.AddRange(newUserReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserReleaseRoles;
    }

    public async Task<UserReleaseRole?> GetById(Guid userReleaseRoleId, CancellationToken cancellationToken = default)
    {
        return await Query(ResourceRoleFilter.All)
            .SingleOrDefaultAsync(urr => urr.Id == userReleaseRoleId, cancellationToken);
    }

    public async Task<UserReleaseRole?> GetByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<UserReleaseRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly) =>
        resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => contentDbContext.UserReleaseRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => contentDbContext.UserReleaseRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => contentDbContext.UserReleaseRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => contentDbContext.UserReleaseRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
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
        var userReleaseRoles = await Query(ResourceRoleFilter.All)
            .Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await RemoveMany(userReleaseRoles, cancellationToken);
    }

    public async Task<bool> UserHasRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(role)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<ReleaseRole>() : rolesToInclude;

        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<ReleaseRole>() : rolesToInclude;

        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task MarkEmailAsSent(
        Guid userReleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userReleaseRole = await GetById(userReleaseRoleId: userReleaseRoleId, cancellationToken: cancellationToken);

        if (userReleaseRole is null)
        {
            throw new InvalidOperationException($"No User Release Role found with ID {userReleaseRoleId}.");
        }

        userReleaseRole.EmailSent = dateSent ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }
}
