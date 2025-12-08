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

    Task CreateManyIfNotExists(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    );

    Task<UserPublicationRole?> GetUserPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );

    IQueryable<UserPublicationRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly);

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
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );
}
