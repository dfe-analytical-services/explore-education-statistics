using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStorageUtils
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum FileSizeUnit : byte
        {
            B,
            Kb,
            Mb,
            Gb,
            Tb
        }

        public static IDictionary<string, string> GetDataFileMetaValues(
            string name,
            string metaFileName,
            string userName,
            int numberOfRows)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.NameKey, name},
                {BlobInfoExtensions.MetaFileKey, metaFileName.ToLower()},
                {BlobInfoExtensions.UserNameKey, userName},
                {BlobInfoExtensions.NumberOfRowsKey, numberOfRows.ToString()}
            };
        }

        public static IDictionary<string, string> GetMetaDataFileMetaValues(
            string dataFileName,
            string userName,
            int numberOfRows)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.DataFileKey, dataFileName.ToLower()},
                {BlobInfoExtensions.UserNameKey, userName},
                {BlobInfoExtensions.NumberOfRowsKey, numberOfRows.ToString()}
            };
        }

        public static async Task<CloudBlobContainer> GetCloudBlobContainerAsync(string storageConnectionString,
            string containerName, BlobContainerPermissions permissions = null, BlobRequestOptions requestOptions = null)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (requestOptions != null)
            {
                blobClient.DefaultRequestOptions = requestOptions;
            }

            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            if (permissions != null)
            {
                await blobContainer.SetPermissionsAsync(permissions);
            }

            return blobContainer;
        }

        public static async Task<CloudBlockBlob> UploadFromStreamAsync(string storageConnectionString, string containerName,
            string blobName, string contentType, string content, BlobRequestOptions requestOptions = null)
        {
            var blobContainer = await GetCloudBlobContainerAsync(storageConnectionString,
                containerName,
                requestOptions: requestOptions);

            var blob = blobContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = contentType;

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                await blob.UploadFromStreamAsync(stream);
            }

            return blob;
        }

        public static string GetSize(long contentLength)
        {
            var fileSize = contentLength;
            var unit = FileSizeUnit.B;
            while (fileSize >= 1024 && unit < FileSizeUnit.Tb)
            {
                fileSize /= 1024;
                unit++;
            }

            return $"{fileSize:0.##} {unit}";
        }

        public static bool IsBatchedDataFile(IListBlobItem blobItem, Guid releaseId)
        {
            return blobItem.Parent.Prefix.Equals(AdminReleaseBatchesDirectoryPath(releaseId));
        }

        public static bool IsMetaDataFile(CloudBlob blob)
        {
            // The meta data file contains a metadata attribute referencing it's corresponding data file
            return blob.Metadata.ContainsKey(BlobInfoExtensions.DataFileKey);
        }

        public static int CalculateNumberOfRows(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                var numberOfLines = 0;
                while (reader.ReadLine() != null)
                {
                    ++numberOfLines;
                }

                return numberOfLines;
            }
        }

        public static int GetNumBatches(int rows, int rowsPerBatch)
        {
            return (int) Math.Ceiling(rows / (double) rowsPerBatch);
        }
    }
}