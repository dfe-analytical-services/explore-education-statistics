#nullable enable
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class PublicBlobStorageService : BlobStorageService, IPublicBlobStorageService
    {
        public PublicBlobStorageService(
            ILogger<PrivateBlobStorageService> logger,
            IConfiguration configuration)
        {
            ConnectionString = configuration.GetValue<string>("PublicStorage");
            Client = new BlobServiceClient(ConnectionString);
            Logger = logger;
            StorageInstanceCreationUtil = new StorageInstanceCreationUtil();
        }
    }
}
