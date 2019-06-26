using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task CopyReleaseToPublicContainer(PublishReleaseDataMessage message);
    }
}