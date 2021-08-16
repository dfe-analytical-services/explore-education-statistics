#nullable enable
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
            Image,
            DataGuidance
        };

        public static string Path(this File file)
        {
            return $"{FilesPath(file.RootPath, file.Type)}{file.Id}";
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
            return file.PublicPath(release.Id);
        }

        public static string PublicPath(this File file, Guid releaseId)
        {
            if (!PublicFileTypes.Contains(file.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(file.Type), file.Type, "Cannot create public path for file type");
            }

            // Public Release files are located in blob storage on path /<releaseId>/<type>/...
            // where Release is that of the latest Release version.
            // This is not necessarily the same as the Release at the time of uploading,
            // if the file belongs to a Release amendment.

            return $"{FilesPath(releaseId, file.Type)}{file.Id}";
        }

        public static string ZipFileEntryName(this File file)
        {
            return file.Type.GetEnumLabel() + "/" + file.Filename;
        }

        // TODO: Remove after completion of EES-2343
        public static FileInfo ToFileInfoNotFound(this File file)
        {
            return new FileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                Name = FileInfo.UnknownName,
                Size = FileInfo.UnknownSize,
                Type = file.Type
            };
        }
    }
}
