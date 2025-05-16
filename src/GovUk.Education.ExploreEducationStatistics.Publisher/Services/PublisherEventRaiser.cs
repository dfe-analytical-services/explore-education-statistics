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
    /// <param name="publishedReleaseVersionInfos">information about the one or more release versions that have been published</param>
    public async Task RaiseReleaseVersionPublishedEvents(
        IEnumerable<ReleaseVersionPublishedEvent.PublishedReleaseVersionInfo> publishedReleaseVersionInfos) =>
        await eventRaiser.RaiseEvents(
            publishedReleaseVersionInfos.Select(info => new ReleaseVersionPublishedEvent(info)));
}
