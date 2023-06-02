#nullable enable
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class PrivateBlobStorageService : BlobStorageService, IPrivateBlobStorageService
    {
        public PrivateBlobStorageService(
            ILogger<PrivateBlobStorageService> logger,
            IConfiguration configuration)
        {
            ConnectionString = configuration.GetValue<string>("CoreStorage");
            Client = new BlobServiceClient(ConnectionString);
            Logger = logger;
            StorageInstanceCreationUtil = new StorageInstanceCreationUtil();
        }
    }
}
