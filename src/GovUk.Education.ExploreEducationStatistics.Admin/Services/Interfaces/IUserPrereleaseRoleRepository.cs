#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPrereleaseRoleRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPrereleaseRoleRepository
{
    Task<UserReleaseRole> Create(
        Guid userId,
        Guid releaseVersionId,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task<List<UserReleaseRole>> CreateManyIfNotExists(
        IEnumerable<UserPrereleaseRoleCreateDto> userPrereleaseRolesToCreate,
        CancellationToken cancellationToken = default
    );

    Task<UserReleaseRole?> GetById(Guid userPrereleaseRoleId, CancellationToken cancellationToken = default);

    Task<UserReleaseRole?> GetByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// <para>
    /// Return a queryable representation of all user release roles, with the option to filter by their status (see <see cref="ResourceRoleFilter"/>).
    /// </para>
    /// </summary>
    /// <param name="resourceRoleFilter">Filter resource roles by their status (see <see cref="ResourceRoleFilter"/>).</param>
    IQueryable<UserReleaseRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly);

    Task<bool> RemoveById(Guid userPrereleaseRoleId, CancellationToken cancellationToken = default);

    Task<bool> RemoveByCompositeKey(Guid userId, Guid releaseVersionId, CancellationToken cancellationToken = default);

    Task RemoveMany(IEnumerable<UserReleaseRole> userPrereleaseRoleIds, CancellationToken cancellationToken = default);

    Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> UserHasPrereleaseRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    );

    Task<bool> UserHasPrereleaseRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    );

    Task MarkEmailAsSent(
        Guid userPrereleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    );
}
