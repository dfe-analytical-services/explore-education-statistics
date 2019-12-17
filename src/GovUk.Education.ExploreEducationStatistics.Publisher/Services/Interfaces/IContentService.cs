using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentService
    {
        Task UpdateDownloadTree();
        Task UpdatePublicationTree();
        Task UpdateMethodologyTree();
        Task UpdatePublicationsAndReleases();
        Task UpdateMethodologies();
    }
}