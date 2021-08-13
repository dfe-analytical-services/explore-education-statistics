#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<IList<ThemeTree<PublicationTreeNode>>> GetPublicationTree();

        Task<IList<ThemeTree<PublicationDownloadsTreeNode>>> GetPublicationDownloadsTree();
    }
}