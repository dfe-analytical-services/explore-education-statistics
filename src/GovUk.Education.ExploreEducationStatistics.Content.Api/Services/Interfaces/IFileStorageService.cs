using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> DownloadTextAsync(string blobName);
    }
}