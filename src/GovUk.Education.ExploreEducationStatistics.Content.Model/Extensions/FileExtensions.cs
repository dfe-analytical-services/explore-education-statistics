﻿using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class FileExtensions
    {
        public static string Path(this ReleaseFile releaseFile)
        {
            return releaseFile.ReleaseFileReference.Path();
        }

        public static string Path(this ReleaseFileReference file)
        {
            return AdminReleasePath(
                file.ReleaseId,
                file.ReleaseFileType,
                file.BlobStorageName);
        }

        public static string PublicPath(this ReleaseFile releaseFile)
        {
            return releaseFile.ReleaseFileReference.PublicPath(releaseFile.Release);
        }

        public static string PublicPath(this ReleaseFileReference file, Release release)
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
                file.ReleaseFileType,
                file.BlobStorageName);
        }

        public static FileInfo ToFileInfo(this ReleaseFileReference file, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                // EES-1637 Prefer name field on blob
                Name = blobInfo.Name.IsNullOrEmpty() ? file.Filename : blobInfo.Name,
                Path = file.Path(),
                Size = blobInfo.Size,
                Type = file.ReleaseFileType
            };
        }

        public static FileInfo ToFileInfoNotFound(this ReleaseFileReference file)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                Name = "Unknown",
                Path = null,
                Size = "0.00 B",
                Type = file.ReleaseFileType
            };
        }

        public static FileInfo ToPublicFileInfo(this ReleaseFileReference file, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                Name = blobInfo.Name,
                Path = blobInfo.Path,
                Size = blobInfo.Size,
                Type = file.ReleaseFileType
            };
        }
    }
}