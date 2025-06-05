#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IAdminEventRaiser
{
    Task OnThemeUpdated(Theme theme);

    Task OnReleaseSlugChanged(
        Guid releaseId,
        string newReleaseSlug,
        Guid publicationId,
        string publicationSlug);

    Task OnPublicationArchived(
        Guid publicationId,
        string publicationSlug,
        Guid supersededByPublicationId);

    Task OnPublicationChanged(Publication publication);

    Task OnPublicationLatestPublishedReleaseReordered(
        Publication publication,
        Guid previousLatestPublishedReleaseId,
        Guid previousLatestPublishedReleaseVersionId);

    Task OnPublicationRestored(
        Guid publicationId,
        string publicationSlug,
        Guid previousSupersededByPublicationId);
}
