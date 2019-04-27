using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json.Linq;
    using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

    public class Processor
    {
        private const string StorageConnectionName = "BlobStorageConnString";
        private const string ContainerName = "releases";
        private const string UploadsDir = "admin-file-uploads";

        [FunctionName("FilesProcessor")]
        public void FilesProcessorFunc(
            [QueueTrigger("filesprocessor-queue", Connection = "")]
            JObject fNotify,
            [Inject]IImporterService importerService,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function processed: {fNotify.ToString()}");

            log.LogInformation($"importerService: {importerService}");

            var filesProcessorNotification = ExtractNotification(fNotify);
            var config = LoadAppSettings(context);
            var connectionStr = config.GetConnectionString(StorageConnectionName);

            ProcessFiles(filesProcessorNotification, connectionStr, log);

            log.LogInformation("Completed files processing");
        }

        public void ProcessFiles(ProcessorNotification processorNotification, string connectionStr, ILogger log)
        {
            if (CloudStorageAccount.TryParse(connectionStr, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(ContainerName);

                    log.LogInformation("Process files if container exists");

                    if (blobContainer != null)
                    {
                        log.LogInformation("Found blob container");

                        ListBlobs(blobContainer, UploadsDir + "/" + processorNotification.PublicationId, log);
                    }
                }
                catch (StorageException ex)
                {
                    log.LogError("Error returned from the service: {0}", ex.Message);
                }
            }
        }

        public async void ListBlobs(CloudBlobContainer blobContainer, string directory, ILogger log) {

            var publicationUploadDir = blobContainer.GetDirectoryReference(directory);

            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var results = await publicationUploadDir.ListBlobsSegmentedAsync(blobContinuationToken);

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    var filename = GetFileName(item);
                    log.LogInformation($"after conversion {filename}");

                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(item.Uri.ToString());
                    /*
                    using (StreamReader reader = new StreamReader(blockBlob.OpenReadAsync()))
                    {
                        oldContent = reader.ReadToEnd();
                    }
                    */
                }
            }
            while (blobContinuationToken != null); // Loop while the continuation token is not null.
        }

        private static ProcessorNotification ExtractNotification(JObject processorNotification)
        {
            return processorNotification.ToObject<ProcessorNotification>();
        }

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static string GetFileName(IListBlobItem item)
        {
            var filename = System.IO.Path.GetFileName(item.Uri.AbsolutePath);
            var lastindex = filename.LastIndexOf('.');
            if (lastindex != -1)
            {
                filename = filename.Substring(0, lastindex);
            }

            return filename;
        }
    }
}
