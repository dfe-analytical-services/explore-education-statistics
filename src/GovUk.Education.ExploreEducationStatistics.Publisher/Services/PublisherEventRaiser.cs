using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Events;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

/// <summary>
/// Publish events specific to the Publisher
/// </summary>
public class PublisherEventRaiser(IEventRaiser eventRaiser) : IPublisherEventRaiser
{
    /// <summary>
    /// Publishes events for release versions that have been published.
    /// </summary>
    /// <param name="publishedPublications">A list of publications, each containing information about the publication
    /// and its associated release versions that have been published.
    /// </param>
    public async Task OnReleaseVersionsPublished(
        IReadOnlyList<PublishedPublicationInfo> publishedPublications)
    {
        var events = publishedPublications.SelectMany(publication =>
            publication.PublishedReleaseVersions.Select(releaseVersion =>
                new ReleaseVersionPublishedEvent(
                    new ReleaseVersionPublishedEvent.ReleaseVersionPublishedEventInfo
                    {
                        ReleaseId = releaseVersion.ReleaseId,
                        ReleaseSlug = releaseVersion.ReleaseSlug,
                        ReleaseVersionId = releaseVersion.ReleaseVersionId,
                        PublicationId = publication.PublicationId,
                        PublicationSlug = publication.PublicationSlug,
                        PreviousLatestReleaseId = publication.PreviousLatestPublishedReleaseId,
                        PublicationLatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId
                    })))
            .ToList();

        await eventRaiser.RaiseEvents(events);
    }
}
