using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;

namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
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
            [Inject]ISeedService seedService,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function processed: {fNotify.ToString()}");

            log.LogInformation($"seedService: {seedService}");

            var filesProcessorNotification = ExtractNotification(fNotify);
            var config = LoadAppSettings(context);
            var connectionStr = config.GetConnectionString(StorageConnectionName);

            ProcessFiles(seedService, filesProcessorNotification, connectionStr, log);

            log.LogInformation("Completed files processing");
        }

        public void ProcessFiles(ISeedService seedService, ProcessorNotification processorNotification, string connectionStr, ILogger log)
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
                        var subjects = GetSubjects(
                            blobContainer,
                            UploadsDir + "/" + processorNotification.PublicationId,
                            log).Result;

                        var publication = CreatePublication(processorNotification, subjects);

                        //seedService.Seed(publication);
                    }
                }
                catch (StorageException ex)
                {
                    log.LogError("Error returned from the service: {0}", ex.Message);
                }
            }
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

        private static async Task<Subject[]> GetSubjects(CloudBlobContainer blobContainer, string directory, ILogger log)
        {
            var publicationUploadDir = blobContainer.GetDirectoryReference(directory);
            List<Subject> list = new List<Subject>();

            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var results = await publicationUploadDir.ListBlobsSegmentedAsync(blobContinuationToken);

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;

                foreach (IListBlobItem item in results.Results)
                {
                    var filename = GetFileName(item);

                    log.LogInformation($"add subject {filename}");

                    if (!filename.StartsWith("meta"))
                    {
                        CloudBlockBlob csvBlockBlob =
                            blobContainer.GetBlockBlobReference(directory + "/" + filename + ".csv");
                        CloudBlockBlob csvMetaBlockBlob =
                            blobContainer.GetBlockBlobReference(directory + "/meta_" + filename + ".csv");

                        list.Add(new Subject()
                        {
                            Name = filename,
                            CsvDataBlob = csvBlockBlob,
                            CsvMetaDataBlob = csvMetaBlockBlob
                        });
                        /*
                        using (StreamReader reader = new StreamReader(await csvBlockBlob.OpenReadAsync()))
                        {
                            var content = reader.ReadToEnd();
                        }
                        */
                    }
                }
            }
            while (blobContinuationToken != null); // Loop while the continuation token is not null.

            return list.ToArray();
        }

        private static Publication CreatePublication(
            ProcessorNotification processorNotification,
            Subject[] subjects)
        {
            return new Publication
            {
                PublicationId = new Guid(processorNotification.PublicationId),
                Name = processorNotification.PublicationName,
                Releases = new[]
                {
                    new Release
                    {
                        PublicationId = new Guid(processorNotification.PublicationId),
                        ReleaseDate = DateTime.UtcNow,
                        Name = processorNotification.ReleaseName,
                        Subjects = subjects
                    }
                }
            };
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
