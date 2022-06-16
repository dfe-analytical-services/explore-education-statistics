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
        [Obsolete("EES-2865 - Remove with other fast track code")]
        public const string FastTrackResultsDirectory = "fast-track-results";
        public const string ReleasesDirectory = "releases";
        public const string SubjectMetaDirectory = "subject-meta";

        public static string PublicContentStagingPath()
        {
            return "staging";
        }

        private static string PublicContentFastTrackPath(string? prefix = null)
        {
            return $"{AppendPathSeparator(prefix)}fast-track";
        }

        private static string PublicContentPublicationsPath(string? prefix = null)
        {
            return $"{AppendPathSeparator(prefix)}publications";
        }

        public static string PublicContentReleaseFastTrackPath(string releaseId, string? prefix = null)
        {
            return $"{PublicContentFastTrackPath(prefix)}/{releaseId}";
        }

        public static string PublicContentFastTrackPath(string releaseId, string id, string? prefix = null)
        {
            return $"{PublicContentReleaseFastTrackPath(releaseId, prefix)}/{id}.json";
        }

        private static string PublicContentPublicationParentPath(string slug, string? prefix = null)
        {
            return $"{PublicContentPublicationsPath(prefix)}/{slug}";
        }

        private static string PublicContentReleaseParentPath(string publicationSlug,
            string releaseSlug,
            string? prefix = null)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug, prefix)}/{ReleasesDirectory}/{releaseSlug}";
        }

        public static string PublicContentPublicationPath(string slug, string? prefix = null)
        {
            return $"{PublicContentPublicationParentPath(slug, prefix)}/publication.json";
        }

        public static string PublicContentLatestReleasePath(string slug, string? prefix = null)
        {
            return $"{PublicContentPublicationParentPath(slug, prefix)}/latest-release.json";
        }

        public static string PublicContentReleasePath(string publicationSlug, string releaseSlug, string? prefix = null)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug, prefix)}/{ReleasesDirectory}/{releaseSlug}.json";
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

        public static string PublicContentFastTrackResultsParentPath(string publicationSlug, string releaseSlug)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/{FastTrackResultsDirectory}";
        }

        public static string PublicContentFastTrackResultsPath(string publicationSlug, string releaseSlug,
            Guid fastTrackId)
        {
            return $"{PublicContentFastTrackResultsParentPath(publicationSlug, releaseSlug)}/{fastTrackId}.json";
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
