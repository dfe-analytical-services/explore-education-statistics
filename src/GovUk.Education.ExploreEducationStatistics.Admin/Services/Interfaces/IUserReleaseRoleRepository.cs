#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserReleaseRoleRepository
{
    Task<UserReleaseRole> Create(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task<List<UserReleaseRole>> CreateManyIfNotExists(
        IReadOnlyList<UserReleaseRole> userReleaseRolesToCreate,
        CancellationToken cancellationToken = default
    );

    Task<UserReleaseRole?> GetById(Guid userReleaseRoleId, CancellationToken cancellationToken = default);

    Task<UserReleaseRole?> GetByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// <para>
    /// Return a queryable representation of all user release roles, with the option to filter by their status (see <see cref="ResourceRoleFilter"/>).
    /// </para>
    /// </summary>
    /// <param name="resourceRoleFilter">Filter resource roles by their status (see <see cref="ResourceRoleFilter"/>).</param>
    IQueryable<UserReleaseRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly);

    Task<bool> RemoveById(Guid userReleaseRoleId, CancellationToken cancellationToken = default);

    Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    );

    Task RemoveMany(HashSet<Guid> userReleaseRoleIds, CancellationToken cancellationToken = default);

    Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> UserHasRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    );

    Task<bool> UserHasAnyRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    );

    Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    );

    Task MarkEmailAsSent(
        Guid userReleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    );
}
