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
    /// On Release Version Published
    /// </summary>
    /// <param name="publishedReleaseVersionInfos">information about the one or more release versions that have been published</param>
    public async Task RaiseReleaseVersionPublishedEvents(
        IEnumerable<PublishedReleaseVersionInfo> publishedReleaseVersionInfos) =>
        await eventRaiser.RaiseEvents(
            publishedReleaseVersionInfos.Select(info => new ReleaseVersionPublishedEvent(
                new ReleaseVersionPublishedEvent.ReleaseVersionPublishedEventInfo
                {
                    PublicationId = info.PublicationId,
                    PublicationSlug = info.PublicationSlug,
                    ReleaseId = info.ReleaseId,
                    ReleaseSlug = info.ReleaseSlug,
                    ReleaseVersionId = info.ReleaseVersionId,
                    PublicationLatestPublishedReleaseVersionId = info.PublicationLatestPublishedReleaseVersionId
                })));
}
