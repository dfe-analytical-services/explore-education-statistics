using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

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

        private const string NameKey = "name";
        
        public const string NumberOfRows = "NumberOfRows";

        public const string UserName = "userName";
        
        /**
         * Property key on a metadata file to point at the data file
         */
        public const string DataFileKey = "datafile";
        
        /**
         * Property key on a data file to point at the metadata file
         */
        public const string MetaFileKey = "metafile";
        
        public static async Task<CloudBlobContainer> GetCloudBlobContainerAsync(string storageConnectionString,
            string containerName, BlobContainerPermissions permissions = null)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            if (permissions != null)
            {
                await blobContainer.SetPermissionsAsync(permissions);
            }

            return blobContainer;
        }

        public static CloudBlobContainer GetCloudBlobContainer(string storageConnectionString, string containerName,
            BlobContainerPermissions permissions = null)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExists();

            if (permissions != null)
            {
                blobContainer.SetPermissions(permissions);
            }

            return blobContainer;
        }
        
        public static IEnumerable<FileInfo> ListPublicFiles(string storageConnectionString, string containerName, string publication, string release)
        {
            var files = new List<FileInfo>();

            files.AddRange(ListFiles(storageConnectionString, containerName, publication, release, ReleaseFileTypes.Data));
            files.AddRange(ListFiles(storageConnectionString, containerName, publication, release, ReleaseFileTypes.Ancillary));

            return files.OrderBy(f => f.Name);
        }

        private static IEnumerable<FileInfo> ListFiles(string storageConnectionString, string containerName, string publication, string release, ReleaseFileTypes type)
        {
            var blobContainer = GetCloudBlobContainer(storageConnectionString, containerName);

            var result = blobContainer
                .ListBlobs(FileStoragePathUtils.PublicReleaseDirectoryPath(publication, release, type), true,
                    BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Where(IsFileReleased)
                .Select(file => new FileInfo
                {
                    Extension = GetExtension(file),
                    Name = GetName(file),
                    Path = file.Name,
                    Size = GetSize(file)
                })
                .OrderBy(info => info.Name).ToList();
            return result;
        }
        
        public static string GetExtension(CloudBlob blob)
        {
            return Path.GetExtension(blob.Name).TrimStart('.');
        }
        
        public static string GetName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(NameKey, out var name) ? name : string.Empty;
        }

        public static string GetSize(CloudBlob blob)
        {
            var fileSize = blob.Properties.Length;
            var unit = FileSizeUnit.B;
            while (fileSize >= 1024 && unit < FileSizeUnit.Tb)
            {
                fileSize /= 1024;
                unit++;
            }

            return $"{fileSize:0.##} {unit}";
        }
        
        public static bool IsFileReleased(CloudBlob blob)
        {
            if (!blob.Exists())
            {
                throw new ArgumentException("File not found: {filename}", blob.Name);
            }

            if (blob.Metadata.TryGetValue("releasedatetime", out var releaseDateTime))
            {
                return DateTime.Compare(ParseDateTime(releaseDateTime), DateTime.Now) <= 0;
            }

            return false;
        }
        
        public static bool IsMetaDataFile(CloudBlob blob)
        {
            // The meta data file contains a metadata attribute referencing it's corresponding data file
            return blob.Metadata.ContainsKey(DataFileKey);
        }
        
        private static DateTime ParseDateTime(string dateTime)
        {
            return DateTime.ParseExact(dateTime, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}