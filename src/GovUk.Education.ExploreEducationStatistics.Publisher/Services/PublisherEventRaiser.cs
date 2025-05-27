using System;
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
    /// Publishes an event when a publication is archived.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication that has been archived.</param>
    /// <param name="publicationSlug">The slug of the publication that has been archived.</param>
    /// <param name="supersededByPublicationId">The unique identifier of the publication that has superseded the archived publication.</param>
    public async Task OnPublicationArchived(
        Guid publicationId,
        string publicationSlug,
        Guid supersededByPublicationId) =>
        await eventRaiser.RaiseEvent(new PublicationArchivedEvent(
            publicationId,
            publicationSlug,
            supersededByPublicationId));

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
                        PreviousLatestPublishedReleaseId = publication.PreviousLatestPublishedReleaseId,
                        PreviousLatestPublishedReleaseVersionId = publication.PreviousLatestPublishedReleaseVersionId,
                        LatestPublishedReleaseId = publication.LatestPublishedReleaseId,
                        LatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId
                    })))
            .ToList();

        await eventRaiser.RaiseEvents(events);
    }
}
