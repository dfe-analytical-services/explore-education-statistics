using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Files;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private const string ContainerName = "releases";

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("CoreStorage");
        }

        public async Task<SubjectData> GetSubjectData(ImportMessage importMessage)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);

            var publication = importMessage.Release.Publication.Slug;
            var release = importMessage.Release.Slug;

            var dataBlob = blobContainer.GetBlockBlobReference(
                $"{publication}/{release}/{importMessage.DataFileName}");

            await dataBlob.FetchAttributesAsync();

            var metaBlob = blobContainer.GetBlockBlobReference(
                $"{publication}/{release}/{BlobUtils.GetMetaFileName(dataBlob)}");

            return new SubjectData(dataBlob, metaBlob, BlobUtils.GetName(dataBlob));
        }
        
        public async Task<Boolean> UploadDataFileAsync(string publication, string release, IFormFile dataFile, string metaFileName,
            string name)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await blobContainer.SetPermissionsAsync(permissions);
            
            await UploadFileAsync(blobContainer, publication, release, dataFile, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("metafile", metaFileName)
            });
            return true;
        }

        private static async Task UploadFileAsync(CloudBlobContainer blobContainer, string publication, string release,
            IFormFile file, IEnumerable<KeyValuePair<string, string>> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference($"{publication}/{release}/{file.FileName}");
            blob.Properties.ContentType = file.ContentType;
            var path = await FileUtils.UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);
            await AddMetaValuesAsync(blob, metaValues);
        }

        private static async Task AddMetaValuesAsync(CloudBlob blob, IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var value in values)
            {
                blob.Metadata.Add(value);
            }

            await blob.SetMetadataAsync();
        }
    }
}