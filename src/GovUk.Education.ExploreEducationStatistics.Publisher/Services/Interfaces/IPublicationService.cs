using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<PublicationViewModel> GetViewModelAsync(Guid id, IEnumerable<Guid> includedReleaseIds);
        List<ThemeTree<PublicationTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds);
        IEnumerable<Publication> ListPublicationsWithPublishedReleases();
    }
}