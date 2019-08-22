using System.Collections;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces
{
    public interface IPublicationService
    {
        PublicationViewModel GetPublication(string slug);

        IEnumerable<Publication> ListPublicationsWithPublishedReleases();
    }
}