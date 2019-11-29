using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStoragePathUtils
    {
        public const string BatchesDir = "batches";

        /**
         * The top level admin directory path where files on a release are stored.
         */
        public static string AdminReleaseDirectoryPath(string releaseId)
        {
            return $"{releaseId}/";
        }

        /**
         * The admin directory path where files, of a particular type, on a release are stored.
         */
        public static string AdminReleaseDirectoryPath(string releaseId, ReleaseFileTypes type)
        {
            return AdminReleaseDirectoryPath(releaseId) + $"{type.GetEnumLabel()}/";
        }

        /**
         * The admin directory path where data files are batched for importing
         */
        public static string AdminReleaseBatchesDirectoryPath(Guid releaseId)
        {
            return AdminReleaseBatchesDirectoryPath(releaseId.ToString());
        }

        /**
         * The admin directory path where data files are batched for importing
         */
        public static string AdminReleaseBatchesDirectoryPath(string releaseId)
        {
            return AdminReleaseDirectoryPath(releaseId) + $"{ReleaseFileTypes.Data.GetEnumLabel()}/{BatchesDir}/";
        }

        /**
         * The admin file path, for a file of a particular type and name, on a release.
         */
        public static string AdminReleasePath(string releaseId, ReleaseFileTypes type, string fileName)
        {
            return AdminReleaseDirectoryPath(releaseId, type) + $"{fileName}";
        }

        /**
         * The top level admin directory path where files on a release are stored.
         */
        public static string AdminReleaseDirectoryPath(Guid releaseId) =>
            AdminReleaseDirectoryPath(releaseId.ToString());

        /**
         * The admin file path, for a file of a particular type and name, on a release.
         */
        public static string AdminReleasePath(Guid releaseId, ReleaseFileTypes type, string fileName)
            => AdminReleasePath(releaseId.ToString(), type, fileName);

        /**
         * The admin directory path where files, of a particular type, on a release are stored.
         */
        public static string AdminReleaseDirectoryPath(Guid releaseId, ReleaseFileTypes type) =>
            AdminReleaseDirectoryPath(releaseId.ToString(), type);


        /**
         * The public file path, for a file of a particular type and name, on a release.
         */
        public static string PublicReleasePath(string publicationSlug, string releaseSlug, ReleaseFileTypes type,
            string fileName)
        {
            return PublicReleaseDirectoryPath(publicationSlug, releaseSlug, type) + $"{fileName}";
        }

        /**
         * The public directory path where files, of a particular type, on a release are stored.
         */
        public static string PublicReleaseDirectoryPath(string publicationSlug, string releaseSlug,
            ReleaseFileTypes type)
        {
            return PublicReleaseDirectoryPath(publicationSlug, releaseSlug) + $"{type.GetEnumLabel()}/";
        }

        /**
         * The top level public directory path where files on a release are stored.
         */
        public static string PublicReleaseDirectoryPath(string publicationSlug, string releaseSlug)
        {
            return $"{publicationSlug}/{releaseSlug}/";
        }
    }
}