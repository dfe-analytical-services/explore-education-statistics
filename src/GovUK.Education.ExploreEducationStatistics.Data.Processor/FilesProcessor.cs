
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    public class FilesProcessor
    {
        private const string StorageConnectionName = "BlobStorageConnString";

        [FunctionName("FilesProcessor")]
        public void FilesProcessorFunc([QueueTrigger("filesupload-queue", Connection = "")]
            JObject fNotify, ILogger log, ExecutionContext context)
        {

            log.LogInformation($"C# Queue trigger function processed: {fNotify.ToString()}");

            var filesProcessorNotification = extractNotification(fNotify);

            var config = LoadAppSettings(context);
            var connectionStr = config.GetConnectionString(StorageConnectionName);

            ProcessFiles(filesProcessorNotification, connectionStr, log);

            log.LogInformation("Completed files processing");
        }

        public void ProcessFiles(FilesProcessorNotification filesProcessorNotification, string connectionStr,
            ILogger log)
        {
            if (CloudStorageAccount.TryParse(connectionStr, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference("releases");

                    log.LogInformation("Process files if container exists");

                    if (blobContainer != null)
                    {
                        log.LogInformation("Starting file upload to blob storage");

                        var releaseDirectory = "admin-file-uploads"; //publicationId;

                        ListBlobs(blobClient, releaseDirectory, log);

                        //var cloudBlockBlob = blobContainer.GetBlockBlobReference(releaseDirectory + "/" + releaseId + "/" + fileName);
                        //await cloudBlockBlob.UploadFromFileAsync(sourceFile);
                    }
                }
                catch (StorageException ex)
                {
                    log.LogError("Error returned from the service: {0}", ex.Message);
                }
            }
        }
        
        // List the blobs in the container.
        public async void ListBlobs(CloudBlobClient blobClient, string directory, ILogger log) {

            log.LogInformation("Listing blobs");
            var blobContainer = blobClient.GetContainerReference(directory);

            BlobContinuationToken blobContinuationToken = null;
                do

            {
                var results = await blobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    log.LogInformation($"Found blob {item.Uri.ToString()}");

                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(item.Uri.ToString());
                    /*
                    using (StreamReader reader = new StreamReader(blockBlob.OpenReadAsync()))
                    {
                        oldContent = reader.ReadToEnd();
                    }
                    */
                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null.
        }

        private FilesProcessorNotification extractNotification(JObject filesProcessorNotification)
        {
            return filesProcessorNotification.ToObject<FilesProcessorNotification>();
        }

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
