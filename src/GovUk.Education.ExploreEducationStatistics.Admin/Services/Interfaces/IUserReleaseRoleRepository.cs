#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserReleaseRoleRepository
{
    Task<UserReleaseRole> Create(Guid userId, Guid releaseVersionId, ReleaseRole role, Guid createdById);

    Task<UserReleaseRole> CreateIfNotExists(Guid userId, Guid releaseVersionId, ReleaseRole role, Guid createdById);

    Task CreateManyIfNotExists(List<Guid> userIds, Guid releaseVersionId, ReleaseRole role, Guid createdById);

    Task CreateManyIfNotExists(Guid userId, List<Guid> releaseVersionIds, ReleaseRole role, Guid createdById);

    Task<List<ReleaseRole>> ListDistinctRolesByUser(Guid userId, bool includeInactiveUsers = false);

    Task<List<ReleaseRole>> ListRolesByUserAndReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        bool includeInactiveUsers = false
    );

    Task<List<ReleaseRole>> ListRolesByUserAndPublication(
        Guid userId,
        Guid publicationId,
        bool includeInactiveUsers = false
    );

    Task<List<UserReleaseRole>> ListUserReleaseRoles(
        Guid releaseVersionId,
        ReleaseRole[]? rolesToInclude,
        bool includeInactiveUsers = false
    );

    Task<bool> HasUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role);

    Task Remove(UserReleaseRole userReleaseRole, CancellationToken cancellationToken = default);

    Task RemoveMany(IReadOnlyList<UserReleaseRole> userReleaseRoles, CancellationToken cancellationToken = default);

    Task RemoveForPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    );

    Task RemoveForPublicationAndUser(
        Guid publicationId,
        Guid userId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    );

    Task RemoveForReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    );

    Task RemoveForReleaseVersionAndUser(
        Guid releaseVersionId,
        Guid userId,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    );

    Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default);

    Task MarkEmailAsSent(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        DateTimeOffset? emailSent = null,
        CancellationToken cancellationToken = default
    );
}
