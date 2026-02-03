#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationRoleRepository
{
    Task<UserPublicationRole> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    );

    Task<List<UserPublicationRole>> CreateManyIfNotExists(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
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

    Task Remove(UserPublicationRole userPublicationRole, CancellationToken cancellationToken = default);

    Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    );

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
