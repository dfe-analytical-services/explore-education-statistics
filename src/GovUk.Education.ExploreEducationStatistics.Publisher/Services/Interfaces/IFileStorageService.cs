using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task CopyReleaseToPublicContainer(string publication, string release);
    }
}