using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        public FileStorageService(string connectionString)
        {
            _storageConnectionString = connectionString;
        }

        public async Task<string> DownloadTextAsync(string blobName)
        {
            return await FileStorageUtils.DownloadTextAsync(_storageConnectionString, PublicContentContainerName, blobName);
        }
    }
}