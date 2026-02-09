#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserResourceRoleNotificationService
{
    Task NotifyUserOfInvite(Guid userId, CancellationToken cancellationToken = default);

    Task NotifyUserOfNewPublicationRole(Guid userPublicationRoleId, CancellationToken cancellationToken = default);

    Task NotifyUserOfNewReleaseRole(Guid userReleaseRoleId, CancellationToken cancellationToken = default);

    Task NotifyUserOfNewContributorRoles(
        HashSet<Guid> userReleaseRoleIds,
        CancellationToken cancellationToken = default
    );

    Task NotifyUserOfNewPreReleaseRole(Guid userReleaseRoleId, CancellationToken cancellationToken = default);
}
