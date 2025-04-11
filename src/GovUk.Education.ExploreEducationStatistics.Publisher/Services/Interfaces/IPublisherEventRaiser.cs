using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublisherEventRaiser
{
    Task RaiseReleaseVersionPublishedEvents(
        IList<PublishingCompletionService.PublishedReleaseVersionInfo> publishedReleaseVersionInfos);
}
