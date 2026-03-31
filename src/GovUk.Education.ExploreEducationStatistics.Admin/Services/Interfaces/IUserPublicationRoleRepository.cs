#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPublicationRoleRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationRoleRepository
{
    Task<UserPublicationRole?> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task<List<UserPublicationRole>> CreateManyIfNotExists(
        IEnumerable<UserPublicationRoleCreateDto> userPublicationRolesToCreate,
        CancellationToken cancellationToken = default
    );

    Task<UserPublicationRole?> GetById(Guid userPublicationRoleId, CancellationToken cancellationToken = default);

    Task<UserPublicationRole?> GetByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// <para>
    /// Return a queryable representation of all user publication roles, with the option to filter by their status (see <see cref="ResourceRoleFilter"/>).
    /// </para>
    /// </summary>
    /// <param name="resourceRoleFilter">Filter resource roles by their status (see <see cref="ResourceRoleFilter"/>).</param>
    /// </param>
    IQueryable<UserPublicationRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly);

    Task<bool> RemoveById(Guid userPublicationRoleId, CancellationToken cancellationToken = default);

    Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );

    Task RemoveMany(IEnumerable<UserPublicationRole> userPublicationRoles, CancellationToken cancellationToken = default);

    Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> UserHasRoleOnPublication(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    );

    Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params PublicationRole[] rolesToInclude
    );

    Task MarkEmailAsSent(
        Guid userPublicationRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    );
}
