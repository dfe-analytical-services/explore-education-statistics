#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserResourceRoleNotificationService
{
    Task NotifyUserOfNewPublicationRole(
        Guid userId,
        Publication publication,
        PublicationRole role,
        CancellationToken cancellationToken = default
    );

    Task NotifyUserOfNewReleaseRole(
        Guid userId,
        ReleaseVersion releaseVersion,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    );
}
