using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Events;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublisherEventRaiser
{
    Task RaiseReleaseVersionPublishedEvents(
        IEnumerable<ReleaseVersionPublishedEvent.PublishedReleaseVersionInfo> publishedReleaseVersionInfos);
}
