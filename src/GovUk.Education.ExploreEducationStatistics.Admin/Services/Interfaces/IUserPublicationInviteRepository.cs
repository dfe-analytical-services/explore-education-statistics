#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationInviteRepository
{
    Task CreateManyIfNotExists(
        List<UserPublicationRoleCreateRequest> userPublicationRoles,
        string email,
        Guid createdById,
        DateTime? createdDate = null
    );

    Task<List<UserPublicationInvite>> GetInvitesByEmail(
        string email,
        CancellationToken cancellationToken = default
    );

    Task Remove(
        Guid publicationId,
        string email,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );

    Task RemoveMany(
        IReadOnlyList<UserPublicationInvite> userPublicationInvites,
        CancellationToken cancellationToken = default
    );

    Task RemoveByUserEmail(string email, CancellationToken cancellationToken = default);
}
