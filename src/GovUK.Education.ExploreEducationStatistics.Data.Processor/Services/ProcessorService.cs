namespace GovUK.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class ProcessorService : IProcessorService
    {
        private readonly ILogger _logger;
        private readonly ISeedService _seedService;

        public ProcessorService(
            ILogger<SeedService> logger,
            ISeedService seedService)
        {
            _logger = logger;
            _seedService = seedService;
        }

        public void ProcessFiles(
            ProcessorNotification processorNotification,
            string containerName,
            string blobStorageConnectionStr,
            string uploadsDir)
        {
            if (CloudStorageAccount.TryParse(blobStorageConnectionStr, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(containerName);

                    if (blobContainer != null)
                    {
                        var subjects = GetSubjects(
                            blobContainer,
                            uploadsDir + "/" + processorNotification.PublicationId,
                            _logger).Result;

                        var publication = CreatePublication(processorNotification, subjects);

                        _seedService.Seed(publication);
                    }
                }
                catch (StorageException ex)
                {
                    _logger.LogError("Error returned from the service: {0}", ex.Message);
                }
            }
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
                        ReleaseDate = DateTime.Parse(processorNotification.ReleaseDate),
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
