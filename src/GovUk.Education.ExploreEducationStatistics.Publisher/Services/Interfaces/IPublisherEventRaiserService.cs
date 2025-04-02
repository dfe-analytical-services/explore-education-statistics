using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublisherEventRaiserService
{
    Task RaiseReleaseVersionPublishedEvents(
        IList<PublishingCompletionService.PublishedReleaseVersionInfo> publishedReleaseVersionInfos);
}
