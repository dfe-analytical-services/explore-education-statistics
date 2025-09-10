#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationRoleRepository
{
    Task<UserPublicationRole> Create(Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById);

    Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId);

    Task<List<PublicationRole>> GetAllRolesByUserAndPublication(
        Guid userId,
        Guid publicationId);

    Task<UserPublicationRole?> GetUserPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role);

    Task<bool> UserHasRoleOnPublication(
        Guid userId,
        Guid publicationId,
        PublicationRole role);

    Task Remove(
        UserPublicationRole userPublicationRole,
        CancellationToken cancellationToken = default);

    Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default);

    Task RemoveForUser(
        Guid userId,
        CancellationToken cancellationToken = default);
}
