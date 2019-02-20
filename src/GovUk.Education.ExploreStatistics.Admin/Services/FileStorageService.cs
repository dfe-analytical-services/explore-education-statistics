using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;

namespace GovUk.Education.ExploreStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private readonly ILogger _logger;

        public FileStorageService(ILogger<FileStorageService> logger, IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("AzureStorage");
            _logger = logger;
        }

        public async Task UploadFileAsync(string sourceFile, string fileName, Guid releaseId)
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var blobContainer = blobClient.GetContainerReference("releases");

                    _logger.LogInformation("Create blob containers if not exist");
                    await blobContainer.CreateIfNotExistsAsync();

                    var permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };

                    _logger.LogInformation("Set blob permissions");
                    await blobContainer.SetPermissionsAsync(permissions);

                    _logger.LogInformation("Starting file upload to blob storage");

                    var releaseDirectory = "admin-file-uploads"; //publicationId;

                    var cloudBlockBlob = blobContainer.GetBlockBlobReference(releaseDirectory + "/" + releaseId + "/" + fileName);
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);
                }
                catch (StorageException ex)
                {
                    _logger.LogError("Error returned from the service: {0}", ex.Message);
                }
            }
        }

        public List<string> ListFiles(string directory)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(directory);

            var list = blobContainer.ListBlobs(useFlatBlobListing: true);
            var listOfFileNames = new List<string>();

            foreach (var blob in list)
            {
                //string bName = blob.Name;
                //long bSize = blob.Properties.Length;
                //string bModifiedOn = blob.Properties.LastModified.ToString();



                listOfFileNames.Add(blob.Uri.ToString());
            }

            return listOfFileNames;
        }
    }
}
