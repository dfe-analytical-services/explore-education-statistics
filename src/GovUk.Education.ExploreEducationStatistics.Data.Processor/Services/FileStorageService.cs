using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const string ContainerName = "releases";
        private readonly CloudBlobContainer _blobContainer;
        private readonly TimeSpan _minimumAcquireLeaseTimeSpan = TimeSpan.FromSeconds(15);

        public FileStorageService(string connectionString)
        {
            _blobContainer = GetOrCreateBlobContainer(connectionString).Result;
        }

        public async Task<SubjectData> GetSubjectData(ImportMessage importMessage)
        {
            var releaseId = importMessage.Release.Id.ToString();

            var dataBlob = _blobContainer.GetBlockBlobReference(
                $"{releaseId}/data/{importMessage.DataFileName}");

            await dataBlob.FetchAttributesAsync();

            var metaBlob = _blobContainer.GetBlockBlobReference(
                $"{releaseId}/data/{BlobUtils.GetMetaFileName(dataBlob)}");

            return new SubjectData(dataBlob, metaBlob, BlobUtils.GetName(dataBlob));
        }

        public async Task<bool> UploadDataFileAsync(
            Guid releaseId,
            MemoryStream stream,
            string metaFileName,
            string name,
            string fileName,
            string contentType)
        {
            await UploadFileAsync(releaseId.ToString(), stream, new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("metafile", metaFileName)
                },
                fileName,
                contentType);
            return true;
        }

        public void Delete(string releaseId, string dataFileName)
        {
            GetCloudBlockBlob(releaseId, dataFileName).DeleteAsync();
        }

        public Task<string> GetLeaseId(CloudBlockBlob cloudBlockBlob)
        {
            return cloudBlockBlob.AcquireLeaseAsync(
                _minimumAcquireLeaseTimeSpan,
                Guid.NewGuid().ToString());
        }

        public CloudBlockBlob GetCloudBlockBlob(string releaseId, string dataFileName)
        {
            return _blobContainer.GetBlockBlobReference($"{releaseId}/data/{dataFileName}");
        }

        private string GenerateCloudLockBlobLeaseName(string blobLeaseNamePrefix)
        {
            return $"{blobLeaseNamePrefix}.lck";
        }

        private async Task UploadFileAsync(
            string releaseId,
            MemoryStream stream,
            IEnumerable<KeyValuePair<string, string>> metaValues,
            string fileName,
            string contentType)
        {
            var blob = _blobContainer.GetBlockBlobReference($"{releaseId}/data/{fileName}");
            blob.Properties.ContentType = contentType;
            stream.Position = 0;
            await blob.UploadFromStreamAsync(stream, stream.Length);
            await AddMetaValuesAsync(blob, metaValues);
        }

        private static async Task AddMetaValuesAsync(CloudBlob blob, IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var value in values) blob.Metadata.Add(value);

            await blob.SetMetadataAsync();
        }

        private static async Task<CloudBlobContainer> GetOrCreateBlobContainer(string storageConnectionString)
        {
            return await FileStorageUtils.GetCloudBlobContainerAsync(storageConnectionString, ContainerName,
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
        }
    }
}