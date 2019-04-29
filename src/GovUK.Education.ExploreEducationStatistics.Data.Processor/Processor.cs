namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
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
        public static void FilesProcessorFunc(
            [QueueTrigger("publications-pending", Connection = "")] JObject fNotify,
            [Queue("publications-processed", Connection = "")] out JObject fNotifyOut,
            [Inject]ISeedService seedService,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function processed: {fNotify.ToString()}");

            var filesProcessorNotification = ExtractNotification(fNotify);
            var config = LoadAppSettings(context);
            var blobStorageConnectionStr = config.GetConnectionString(StorageConnectionName);

            ProcessFiles(seedService, filesProcessorNotification, blobStorageConnectionStr, log);

            log.LogInformation("Completed files processing");

            fNotifyOut = fNotify;
        }

        private static void ProcessFiles(
            ISeedService seedService,
            ProcessorNotification processorNotification,
            string blobStorageConnectionStr,
            ILogger log)
        {
            if (CloudStorageAccount.TryParse(blobStorageConnectionStr, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(ContainerName);

                    if (blobContainer != null)
                    {
                        var subjects = GetSubjects(
                            blobContainer,
                            UploadsDir + "/" + processorNotification.PublicationId,
                            log).Result;

                        var publication = CreatePublication(processorNotification, subjects);

                        seedService.Seed(publication);
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

        private static Publication CreatePublication(
            ProcessorNotification processorNotification,
            Subject[] subjects)
        {
            return new Publication
            {
                PublicationId = Guid.Parse(processorNotification.PublicationId),
                Name = processorNotification.PublicationName,
                Releases = new[]
                {
                    new Release
                    {
                        PublicationId = Guid.Parse(processorNotification.PublicationId),
                        ReleaseDate = DateTime.UtcNow,
                        Name = processorNotification.ReleaseName,
                        Subjects = subjects
                    }
                }
            };
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

                foreach (IListBlobItem item in results.Results.Where(
                    r => Path.GetFileName(r.Uri.AbsolutePath).EndsWith(".csv")
                                      && !Path.GetFileName(r.Uri.AbsolutePath).StartsWith("meta")))
                {
                    var sName = GetSubjectName(item);

                    list.Add(new Subject()
                    {
                        Name = sName,
                        CsvDataBlob = blobContainer.GetBlockBlobReference(directory + "/" + sName + ".csv"),
                        CsvMetaDataBlob = blobContainer.GetBlockBlobReference(directory + "/meta_" + sName + ".csv")
                    });
                }
            }
            while (blobContinuationToken != null); // Loop while the continuation token is not null.

            return list.ToArray();
        }

        private static string GetSubjectName(IListBlobItem item)
        {
            var filename = Path.GetFileName(item.Uri.AbsolutePath);
            var lastindex = filename.LastIndexOf('.');
            if (lastindex != -1)
            {
                filename = filename.Substring(0, lastindex);
            }

            return filename;
        }
    }
}
