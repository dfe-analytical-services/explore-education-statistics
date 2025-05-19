using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublisherEventRaiser
{
    Task OnReleaseVersionsPublished(IReadOnlyList<PublishedPublicationInfo> publishedPublications);
}
