using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly CloudBlobContainer _blobContainer;

        private const string ContainerName = "releases";

        public FileStorageService(string connectionString)
        {
            _blobContainer = GetOrCreateBlobContainer(connectionString).Result;
        }

        public async Task<SubjectData> GetSubjectData(ImportMessage importMessage)
        {
            var releaseId = importMessage.Release.Id.ToString();

            var dataBlob = _blobContainer.GetBlockBlobReference(
                $"{releaseId}/Data/{importMessage.DataFileName}");

            await dataBlob.FetchAttributesAsync();

            var metaBlob = _blobContainer.GetBlockBlobReference(
                $"{releaseId}/Data/{BlobUtils.GetMetaFileName(dataBlob)}");

            return new SubjectData(dataBlob, metaBlob, BlobUtils.GetName(dataBlob));
        }
        
        public async Task<Boolean> UploadDataFileAsync(Guid releaseId, IFormFile dataFile, string metaFileName,
            string name)
        {
            await UploadFileAsync(releaseId.ToString(), dataFile, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("metafile", metaFileName)
            });
            return true;
        }

        public void Delete(ImportMessage importMessage)
        {
            var releaseId = importMessage.Release.Id.ToString();
            var blob = _blobContainer.GetBlockBlobReference($"{releaseId}/Data/{importMessage.DataFileName}");
            blob.DeleteAsync();
        }

        private async Task UploadFileAsync(string releaseId,
            IFormFile file, IEnumerable<KeyValuePair<string, string>> metaValues)
        {
            var blob = _blobContainer.GetBlockBlobReference($"{releaseId}/Data/{file.FileName}");
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

        private async Task<CloudBlobContainer> GetOrCreateBlobContainer(string storageConnectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await blobContainer.SetPermissionsAsync(permissions);
            return blobContainer;
        }
    }
}