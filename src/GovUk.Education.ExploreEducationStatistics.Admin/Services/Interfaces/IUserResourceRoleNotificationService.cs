#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserResourceRoleNotificationService
{
    Task NotifyUserOfInvite(Guid userId, CancellationToken cancellationToken = default);

    Task NotifyUserOfNewPublicationRole(Guid userPublicationRoleId, CancellationToken cancellationToken = default);

    Task NotifyUserOfNewDrafterRole(Guid userPublicationRoleId, CancellationToken cancellationToken = default);

    Task NotifyUserOfNewPreReleaseRole(Guid userPreReleaseRoleId, CancellationToken cancellationToken = default);
}
