using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class FileExtensions
    {
        public static readonly List<FileType> PublicFileTypes = new List<FileType>
        {
            Ancillary,
            Chart,
            Data,
            Image
        };

        public static string Path(this File file)
        {
            return file.PrivateBlobPathMigrated
                ? file.MigratedPath()
                : file.LegacyPrivatePath();
        }

        public static string MigratedPath(this File file)
        {
            return $"{FilesPath(file.RootPath, file.Type)}{file.Id}";
        }

        private static string LegacyPrivatePath(this File file)
        {
            var blobName = file.Type == Ancillary || file.Type == Chart
                ? file.Id.ToString()
                : file.Filename;
            var typeFolder = (file.Type == Metadata ? Data : file.Type).GetEnumLabel();
            return $"{file.RootPath}/{typeFolder}/{blobName}";
        }

        public static string BatchesPath(this File file)
        {
            return $"{file.RootPath}/{Data.GetEnumLabel()}/batches/{file.Id}/";
        }

        public static string BatchPath(this File file, int batchNumber)
        {
            return $"{file.BatchesPath()}{file.Id}_{batchNumber:000000}";
        }

        public static string PublicPath(this File file, Release release)
        {
            if (!PublicFileTypes.Contains(file.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(file.Type), file.Type, "Cannot create public path for file type");
            }

            // Public Release files are located in blob storage on path /<releaseId>/<type>/...
            // where Release is that of the latest Release version.
            // This is not necessarily the same as the Release at the time of uploading,
            // if the file belongs to a Release amendment.

            return file.PublicBlobPathMigrated
                    ? file.MigratedPublicPath(release)
                    : file.LegacyPublicPath(release);
        }

        public static string MigratedPublicPath(this File file, Release release)
        {
            return file.MigratedPublicPath(release.Id);
        }

        public static string MigratedPublicPath(this File file, Guid releaseId)
        {
            return $"{FilesPath(releaseId, file.Type)}{file.Id}";
        }

        private static string LegacyPublicPath(this File file, Release release)
        {
            if (release.Publication == null)
            {
                throw new ArgumentException("Release must be hydrated with Publication to create legacy public path");
            }

            var blobName = file.Type == Ancillary || file.Type == Chart
                ? file.Id.ToString()
                : file.Filename;
            return $"{release.Publication.Slug}/{release.Slug}/{file.Type.GetEnumLabel()}/{blobName}";
        }

        public static FileInfo ToFileInfo(this File file, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                // EES-1815 Change to use Subject name rather than BlobInfo.Name
                Name = blobInfo.Name.IsNullOrEmpty() ? file.Filename : blobInfo.Name,
                Path = file.Path(),
                Size = blobInfo.Size,
                Type = file.Type
            };
        }

        public static FileInfo ToFileInfoNotFound(this File file)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                Name = "Unknown",
                Path = null,
                Size = "0.00 B",
                Type = file.Type
            };
        }

        public static FileInfo ToPublicFileInfo(this File file, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                Name = blobInfo.Name,
                Path = blobInfo.Path,
                Size = blobInfo.Size,
                Type = file.Type
            };
        }
    }
}
