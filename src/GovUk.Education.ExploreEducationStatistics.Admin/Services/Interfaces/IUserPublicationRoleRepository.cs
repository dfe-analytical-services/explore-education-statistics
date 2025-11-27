#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationRoleRepository
{
    Task<UserPublicationRole> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null
    );

    Task CreateManyIfNotExists(IReadOnlyList<UserPublicationRole> userPublicationRoles);

    Task<List<PublicationRole>> ListDistinctRolesByUser(Guid userId, bool includeInactiveUsers = false);

    Task<List<PublicationRole>> ListRolesByUserAndPublication(
        Guid userId,
        Guid publicationId,
        bool includeInactiveUsers = false
    );

    Task<List<UserPublicationRole>> ListRolesForUser(
        Guid userId,
        bool includeInactiveUsers = false,
        params PublicationRole[] rolesToInclude
    );

    Task<List<UserPublicationRole>> ListRolesForPublication(
        Guid publicationId,
        bool includeInactiveUsers = false,
        params PublicationRole[] rolesToInclude
    );

    Task<UserPublicationRole?> GetUserPublicationRole(Guid userId, Guid publicationId, PublicationRole role);

    Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role);

    Task Remove(UserPublicationRole userPublicationRole, CancellationToken cancellationToken = default);

    Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    );

    Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default);

    Task MarkEmailAsSent(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        DateTimeOffset? emailSent = null,
        CancellationToken cancellationToken = default
    );
}
