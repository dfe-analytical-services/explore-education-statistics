using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ProcessorService : IProcessorService
    {
        private readonly ISeedService _seedService;

        public ProcessorService(ISeedService seedService)
        {
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
                            uploadsDir + "/" + processorNotification.PublicationId).Result;

                        var release = CreateRelease(processorNotification, subjects);

                        _seedService.SeedRelease(release);
                    }
                }
                catch (StorageException ex)
                {
                }
            }
        }

        private static Release CreateRelease(
            ProcessorNotification processorNotification,
            Subject[] subjects)
        {
            return new Release
            {
                PublicationId = Guid.Parse(processorNotification.PublicationId),
                ReleaseDate = DateTime.Parse(processorNotification.ReleaseDate),
                Name = processorNotification.ReleaseName,
                Subjects = subjects
            };
        }

        private static async Task<Subject[]> GetSubjects(CloudBlobContainer blobContainer, string directory)
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

                    //logger.LogInformation($"adding subject {sName} to release for import process");

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
