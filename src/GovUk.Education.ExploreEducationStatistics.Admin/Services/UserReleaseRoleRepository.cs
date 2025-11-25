#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleRepository(ContentDbContext contentDbContext, ILogger<UserReleaseRoleRepository> logger)
    : UserResourceRoleRepositoryBase<UserReleaseRoleRepository, UserReleaseRole, ReleaseVersion, ReleaseRole>(
        contentDbContext
    ),
        IUserReleaseRoleRepository
{
    protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceId(Guid releaseVersionId)
    {
        return ContentDbContext.UserReleaseRoles.Where(role => role.ReleaseVersionId == releaseVersionId);
    }

    protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceIds(List<Guid> releaseVersionIds)
    {
        return ContentDbContext.UserReleaseRoles.Where(role => releaseVersionIds.Contains(role.ReleaseVersionId));
    }

    public async Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId)
    {
        return await GetDistinctResourceRolesByUser(userId);
    }

    public async Task<List<ReleaseRole>> GetAllRolesByUserAndReleaseVersion(Guid userId, Guid releaseVersionId)
    {
        return await GetAllResourceRolesByUserAndResource(userId, releaseVersionId);
    }

    public async Task<List<ReleaseRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
    {
        return await ContentDbContext
            .UserReleaseRoles.Where(role =>
                role.UserId == userId && role.ReleaseVersion.Release.PublicationId == publicationId
            )
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

    public new async Task Remove(UserReleaseRole userReleaseRole, CancellationToken cancellationToken = default)
    {
        await base.Remove(userReleaseRole, cancellationToken);
    }

    public new async Task RemoveMany(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        await base.RemoveMany(userReleaseRoles, cancellationToken);
    }

    public async Task RemoveForPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        await RemoveForPublication(
            publicationId: publicationId,
            userId: null,
            cancellationToken: cancellationToken,
            rolesToInclude: rolesToInclude
        );
    }

    public async Task RemoveForPublicationAndUser(
        Guid publicationId,
        Guid userId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        if (userId.IsEmpty())
        {
            logger.LogError(
                "Trying to remove roles/invites for a publication and user combination, "
                    + $"but the supplied '{nameof(userId)}' is EMPTY. '{nameof(userId)}' must not be EMPTY."
            );

            throw new ArgumentException($"{nameof(userId)} must not be EMPTY.", nameof(userId));
        }

        await RemoveForPublication(
            publicationId: publicationId,
            userId: userId,
            cancellationToken: cancellationToken,
            rolesToInclude: rolesToInclude
        );
    }

    public async Task RemoveForReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        await RemoveForReleaseVersion(
            releaseVersionId: releaseVersionId,
            userId: null,
            cancellationToken: cancellationToken,
            rolesToInclude: rolesToInclude
        );
    }

    public async Task RemoveForReleaseVersionAndUser(
        Guid releaseVersionId,
        Guid userId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        if (userId == Guid.Empty)
        {
            logger.LogError(
                "Trying to remove roles/invites for a release version and user combination, "
                    + $"but the supplied '{nameof(userId)}' is EMPTY. '{nameof(userId)}' must not be EMPTY."
            );

            throw new ArgumentException($"{nameof(userId)} must not be EMPTY.", nameof(userId));
        }

        await RemoveForReleaseVersion(
            releaseVersionId: releaseVersionId,
            userId: userId,
            cancellationToken: cancellationToken,
            rolesToInclude: rolesToInclude
        );
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userReleaseRoles = await ContentDbContext
            .UserReleaseRoles.Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await base.RemoveMany(userReleaseRoles, cancellationToken);
    }

    private async Task RemoveForPublication(
        Guid publicationId,
        Guid? userId = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var allReleaseVersionIds = ContentDbContext
            .ReleaseVersions.Where(rv => rv.Release.PublicationId == publicationId)
            .Select(rv => rv.Id)
            .ToHashSet();

        var userReleaseRoles = await ContentDbContext
            .UserReleaseRoles.Where(urr => allReleaseVersionIds.Contains(urr.ReleaseVersionId))
            .If(userId.HasValue)
            .ThenWhere(urr => urr.UserId == userId)
            .If(rolesToInclude.Any())
            .ThenWhere(i => rolesToInclude.Contains(i.Role))
            .ToListAsync(cancellationToken);

        await base.RemoveMany(userReleaseRoles, cancellationToken);
    }

    private async Task RemoveForReleaseVersion(
        Guid releaseVersionId,
        Guid? userId = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var userReleaseRoles = await ContentDbContext
            .UserReleaseRoles.Where(urr => urr.ReleaseVersionId == releaseVersionId)
            .If(userId.HasValue)
            .ThenWhere(urr => urr.UserId == userId)
            .If(rolesToInclude.Any())
            .ThenWhere(i => rolesToInclude.Contains(i.Role))
            .ToListAsync(cancellationToken);

        await base.RemoveMany(userReleaseRoles, cancellationToken);
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
        var userReleaseRole = await GetUserReleaseRole(userId: userId, releaseVersionId: releaseVersionId, role: role);

        if (userReleaseRole is null)
        {
            throw new InvalidOperationException(
                $"No User Release Role found for {nameof(userId)} '{userId}', {nameof(releaseVersionId)} '{releaseVersionId}', {nameof(role)} '{role}'"
            );
        }

        userReleaseRole.EmailSent = emailSent?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        await ContentDbContext.SaveChangesAsync(cancellationToken);
    }
}
