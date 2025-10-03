#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Events;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IAdminEventRaiser
{
    Task OnThemeUpdated(Theme theme);

    Task OnReleaseSlugChanged(
        Guid releaseId,
        string newReleaseSlug,
        Guid publicationId,
        string publicationSlug,
        bool isPublicationArchived
    );

    Task OnPublicationArchived(Guid publicationId, string publicationSlug, Guid supersededByPublicationId);

    Task OnPublicationChanged(Publication publication);

    Task OnPublicationDeleted(
        Guid publicationId,
        string publicationSlug,
        LatestPublishedReleaseInfo? latestPublishedRelease
    );

    Task OnPublicationLatestPublishedReleaseReordered(
        Publication publication,
        Guid previousLatestPublishedReleaseId,
        Guid previousLatestPublishedReleaseVersionId
    );

    Task OnPublicationRestored(Guid publicationId, string publicationSlug, Guid previousSupersededByPublicationId);
}
