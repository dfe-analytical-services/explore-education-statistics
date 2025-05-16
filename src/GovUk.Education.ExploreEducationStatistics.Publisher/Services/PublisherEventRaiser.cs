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
    /// On Release Version Published
    /// </summary>
    /// <param name="publishedPublications">information about the one or more publications that have been published</param>
    public async Task RaiseReleaseVersionPublishedEvents(
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
