using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublicationService
    {
        List<ThemeTree> GetPublicationsTree();
        IEnumerable<Publication> ListPublicationsWithPublishedReleases();
    }
}