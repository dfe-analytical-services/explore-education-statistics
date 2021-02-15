using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class FileExtensions
    {
        public static string Path(this ReleaseFile releaseFile)
        {
            return releaseFile.File.Path();
        }

        public static string Path(this File file)
        {
            return AdminReleasePath(
                file.RootPath,
                file.Type,
                file.BlobStorageName);
        }

        public static string BatchesPath(this File file)
        { 
            return $"{AdminDataFileBatchesDirectoryPath(file.RootPath, file.Id)}";
        }

        public static string BatchPath(this File file, int batchNumber)
        { 
            return $"{file.BatchesPath()}{file.BlobStorageName}_{batchNumber:000000}";
        }

        public static string PublicPath(this ReleaseFile releaseFile)
        {
            return releaseFile.File.PublicPath(releaseFile.Release);
        }

        public static string PublicPath(this File file, Release release)
        {
            if (release.Publication == null)
            {
                throw new ArgumentException("Release must be hydrated with Publication to create public path");
            }
            
            // Files are located in blob storage on path .../<publication-slug>/<release-Slug>/...
            // where Publication slug and Release slug are those of the latest Release version.
            // Don't use file.Release here to locate the blob as that's the Release at the time of uploading,
            // which is not the same if the file belongs to a Release amendment.
            return PublicReleasePath(release.Publication.Slug,
                release.Slug,
                file.Type,
                file.BlobStorageName);
        }

        public static FileInfo ToFileInfo(this File file, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                // EES-1637 Prefer name field on blob
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
