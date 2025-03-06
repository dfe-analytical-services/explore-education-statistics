using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IEventRaiserService
{
    Task RaiseReleaseVersionPublishedEvents(
        IList<PublishingCompletionService.PublishedReleaseVersionInfo> publicationSlugs);
}
