using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const string PublicContentContainerName = "cache";
        private readonly string _storageConnectionString;

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("PublicStorage");
        }

        public async Task<string> DownloadTextAsync(string blobName)
        {
            return await FileStorageUtils.DownloadTextAsync(_storageConnectionString, PublicContentContainerName,
                blobName);
        }
    }
}