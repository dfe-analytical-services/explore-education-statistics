using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublicationService
    {
        IEnumerable<Publication> ListPublicationsWithPublishedReleases();
    }
}