#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStoragePathUtils
    {
        public const string DataBlocksDirectory = "data-blocks";
        public const string ReleasesDirectory = "releases";
        public const string SubjectMetaDirectory = "subject-meta";
        public const string LatestReleaseFileName = "latest-release.json";
        public const string PublicationFileName = "publication.json";

        public static string PublicContentStagingPath() => "staging";

        private static string PublicContentPublicationParentPath(string slug, bool staging = false)
        {
            return $"{AppendPathSeparator(staging ? PublicContentStagingPath() : null)}publications/{slug}";
        }

        private static string PublicContentReleaseParentPath(string publicationSlug, string releaseSlug)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug)}/{ReleasesDirectory}/{releaseSlug}";
        }

        public static string PublicContentPublicationPath(string slug)
        {
            return $"{PublicContentPublicationParentPath(slug)}/{PublicationFileName}";
        }

        public static string PublicContentLatestReleasePath(string publicationSlug, bool staging = false)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug, staging)}/{LatestReleaseFileName}";
        }

        public static string PublicContentReleasePath(string publicationSlug, string releaseSlug, bool staging = false)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug, staging)}/{ReleasesDirectory}/{releaseSlug}.json";
        }

        public static string PublicContentDataBlockParentPath(string publicationSlug, string releaseSlug)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/{DataBlocksDirectory}";
        }

        public static string PrivateContentDataBlockPath(Guid releaseId, Guid dataBlockId)
        {
            return $"{ReleasesDirectory}/{releaseId}/{DataBlocksDirectory}/{dataBlockId}.json";
        }

        public static string PrivateContentSubjectMetaPath(Guid releaseId, Guid subjectId)
        {
            return $"{ReleasesDirectory}/{releaseId}/{SubjectMetaDirectory}/{subjectId}.json";
        }

        public static string PublicContentDataBlockPath(
            string publicationSlug,
            string releaseSlug,
            Guid dataBlockId)
        {
            return $"{PublicContentDataBlockParentPath(publicationSlug, releaseSlug)}/{dataBlockId}.json";
        }

        public static string PublicContentSubjectMetaParentPath(string publicationSlug, string releaseSlug)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/{SubjectMetaDirectory}";
        }

        public static string PublicContentSubjectMetaPath(string publicationSlug, string releaseSlug, Guid subjectId)
        {
            return $"{PublicContentSubjectMetaParentPath(publicationSlug, releaseSlug)}/{subjectId}.json";
        }

        public static string PublicContentReleaseSubjectsPath(string publicationSlug, string releaseSlug)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/subjects.json";
        }

        public static string FilesPath(Guid rootPath, FileType type)
        {
            var typeFolder = (type == Metadata ? Data : type).GetEnumLabel();
            return $"{rootPath}/{typeFolder}/";
        }

        private static string AppendPathSeparator(string? segment = null)
        {
            return segment == null ? "" : segment + "/";
        }
    }
}
