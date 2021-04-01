using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _publisherStorageConnectionString;

        public FileStorageService(string publisherStorageConnectionString)
        {
            _publisherStorageConnectionString = publisherStorageConnectionString;
        }

        public async Task<BlobLease> AcquireLease(string blobName)
        {
            // Ideally we should refactor BlobStorageService to do this, but it doesn't
            // really fit well with our interface. Additionally, we hope to completely scrap
            // table storage in favour of a database table, so we won't need this leasing
            // mechanism in the near future anyway.
            // TODO: Remove this in favour of database table for locking (EES-1232)
            var client = new BlobServiceClient(_publisherStorageConnectionString, new BlobClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Fixed,
                    Delay = TimeSpan.FromSeconds(5),
                    MaxRetries = 5
                }
            });

            var blobContainer = client.GetBlobContainerClient(PublisherLeases.Name);
            await blobContainer.CreateIfNotExistsAsync();

            var blob = blobContainer.GetBlobClient(blobName);
            var blobExists = await blob.ExistsAsync();

            if (!blobExists)
            {
                await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
                await blobContainer.UploadBlobAsync(blobName, stream);
            }

            var leaseClient = blob.GetBlobLeaseClient();
            await leaseClient.AcquireAsync(TimeSpan.FromSeconds(30));

            return new BlobLease(leaseClient);
        }
    }
}
