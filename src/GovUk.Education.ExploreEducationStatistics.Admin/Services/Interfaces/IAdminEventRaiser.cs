using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IAdminEventRaiser
{
    Task OnThemeUpdated(Theme theme);
    Task OnReleaseSlugChanged(Guid releaseId, string newReleaseSlug, Guid publicationId, string publicationSlug);
    Task OnPublicationChanged(Publication publication);
    Task OnPublicationLatestPublishedReleaseVersionChanged(Publication publication, Guid previousLatestPublishedReleaseVersionId);
}
