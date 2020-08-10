using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Publication> Get(Guid id);
        Task<CachedPublicationViewModel> GetViewModelAsync(Guid id, IEnumerable<Guid> includedReleaseIds);
        List<ThemeTree<PublicationTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds);
        IEnumerable<Publication> ListPublicationsWithPublishedReleases();
        Task SetPublishedDate(Guid id, DateTime published);
    }
}