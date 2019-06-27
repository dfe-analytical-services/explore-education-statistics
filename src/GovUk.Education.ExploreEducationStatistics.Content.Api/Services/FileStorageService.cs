using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using MimeTypes;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class FileStorageService : IFileStorageService
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

        private readonly string _storageConnectionString;

        private const string ContainerName = "downloads";

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("PublicStorage");
        }

        public IEnumerable<FileInfo> ListFiles(string publication, string release)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            blobContainer.CreateIfNotExists();

            return blobContainer.ListBlobs($"{publication}/{release}", true, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Where(IsFileReleased)
                .Select(file => new FileInfo
                {
                    Extension = GetExtension(file),
                    Name = GetName(file),
                    Path = file.Name,
                    Size = GetSize(file)
                })
                .OrderBy(info => info.Name);
        }

        private static bool IsFileReleased(CloudBlob blob)
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

        private static string GetExtension(CloudBlob blob)
        {
            var contentType = blob.Properties.ContentType;
            contentType = contentType.Replace("; charset=utf-8", "");
            return MimeTypeMap.GetExtension(contentType).TrimStart('.');
        }

        private static string GetName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue("name", out var name) ? name : "Unknown file name";
        }

        private static string GetSize(CloudBlob blob)
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

        private static DateTime ParseDateTime(string dateTime)
        {
            return DateTime.ParseExact(dateTime, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}