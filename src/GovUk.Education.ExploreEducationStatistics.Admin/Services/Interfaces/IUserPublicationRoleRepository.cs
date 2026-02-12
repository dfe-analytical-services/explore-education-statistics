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
        HashSet<UserPublicationRoleCreateDto> userPublicationRolesToCreate,
        CancellationToken cancellationToken = default
    );

    Task<UserPublicationRole?> GetById(
        Guid userPublicationRoleId,
        bool includeNewPermissionsSystemRoles = false,
        CancellationToken cancellationToken = default
    );

    Task<UserPublicationRole?> GetByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        bool includeNewPermissionsSystemRoles = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// <para>
    /// The optional <paramref name="includeNewPermissionsSystemRoles"/> parameter will be removed once we remove all OLD publication
    /// roles from the DB in STEP 11 (EES-6212) of the Permissions Rework. For now, in certain situations, we need to be able to grab
    /// ALL of the publication roles (NEW &amp; OLD permissions system roles).
    /// </para>
    /// </summary>
    /// <param name="resourceRoleFilter">Filter resource roles by their status (see <see cref="ResourceRoleFilter"/>).</param>
    /// <param name="includeNewPermissionsSystemRoles">
    /// <para>Temporary parameter that controls which roles are included.</para>
    /// <para>When <c>true</c>, includes NEW permissions system roles in addition to OLD roles.</para>
    /// <para>When <c>false</c>, includes only OLD roles.</para>
    /// </param>
    IQueryable<UserPublicationRole> Query(
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        bool includeNewPermissionsSystemRoles = false
    );

    Task<bool> RemoveById(Guid userPublicationRoleId, CancellationToken cancellationToken = default);

    Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );

    Task RemoveMany(HashSet<Guid> userPublicationRoleIds, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// This method is only intended to be used within this class, and by the <see cref="UserReleaseRoleRepository"/>. So although
    /// it is `public`, it is not intended to be used by other external callers. It is necessary to make this `public` for now,
    /// as a temporary measure, but it will be removed in EES-6196, when we no longer have to cater for the old roles,
    /// and the <see cref="UserReleaseRoleRepository"/> will no longer need to call this method.
    /// </summary>
    Task<UserPublicationRole> CreateRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime createdDate,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// This method is only intended to be used within this class, and by the <see cref="UserReleaseRoleRepository"/>. So although
    /// it is `public`, it is not intended to be used by other external callers. It is necessary to make this `public` for now,
    /// as a temporary measure, but it will be removed in EES-6196, when we no longer have to cater for the old roles,
    /// and the <see cref="UserReleaseRoleRepository"/> will no longer need to call this method.
    /// </summary>
    Task RemoveRole(UserPublicationRole userPublicationRole, CancellationToken cancellationToken);
}
