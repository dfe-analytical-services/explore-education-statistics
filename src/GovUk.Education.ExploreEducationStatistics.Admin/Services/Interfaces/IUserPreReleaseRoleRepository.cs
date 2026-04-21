#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPreReleaseRoleRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPreReleaseRoleRepository
{
    Task<UserReleaseRole> Create(
        Guid userId,
        Guid releaseVersionId,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task<List<UserReleaseRole>> CreateManyIfNotExists(
        IEnumerable<UserPreReleaseRoleCreateDto> userPreReleaseRolesToCreate,
        CancellationToken cancellationToken = default
    );

    Task<UserReleaseRole?> GetById(Guid userPreReleaseRoleId, CancellationToken cancellationToken = default);

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

    Task<bool> RemoveById(Guid userPreReleaseRoleId, CancellationToken cancellationToken = default);

    Task<bool> RemoveByCompositeKey(Guid userId, Guid releaseVersionId, CancellationToken cancellationToken = default);

    Task RemoveMany(IEnumerable<UserReleaseRole> userPreReleaseRoleIds, CancellationToken cancellationToken = default);

    Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> UserHasPreReleaseRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    );

    Task<bool> UserHasPreReleaseRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    );

    Task MarkEmailAsSent(
        Guid userPreReleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    );
}
