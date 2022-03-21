using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStoragePathUtils
    {
        public const string PublicContentDataBlocksDirectory = "data-blocks";
        [Obsolete("EES-2865 - Remove with other fast track code")]
        public const string PublicContentFastTrackResultsDirectory = "fast-track-results";
        public const string PublicContentSubjectMetaDirectory = "subject-meta";

        public static string PublicContentStagingPath()
        {
            return "staging";
        }

        private static string PublicContentFastTrackPath(string prefix = null)
        {
            return $"{AppendPathSeparator(prefix)}fast-track";
        }

        private static string PrivateContentPublicationsPath(string prefix = null)
        {
            return $"{AppendPathSeparator(prefix)}publications";
        }

        private static string PublicContentPublicationsPath(string prefix = null)
        {
            return $"{AppendPathSeparator(prefix)}publications";
        }

        public static string PublicContentReleaseFastTrackPath(string releaseId, string prefix = null)
        {
            return $"{PublicContentFastTrackPath(prefix)}/{releaseId}";
        }

        public static string PublicContentFastTrackPath(string releaseId, string id, string prefix = null)
        {
            return $"{PublicContentReleaseFastTrackPath(releaseId, prefix)}/{id}.json";
        }

        public static string PrivateContentPublicationParentPath(Guid publicationId, string prefix = null)
        {
            return $"{PrivateContentPublicationsPath(prefix)}/{publicationId}";
        }

        public static string PublicContentPublicationParentPath(string slug, string prefix = null)
        {
            return $"{PublicContentPublicationsPath(prefix)}/{slug}";
        }

        public static string PrivateContentReleaseParentPath(Guid publicationId, Guid releaseId, string prefix = null)
        {
            return $"{PrivateContentPublicationParentPath(publicationId, prefix)}/releases/{releaseId}";
        }

        public static string PublicContentReleaseParentPath(string publicationSlug, string releaseSlug, string prefix = null)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug, prefix)}/releases/{releaseSlug}";
        }

        public static string PublicContentPublicationPath(string slug, string prefix = null)
        {
            return $"{PublicContentPublicationParentPath(slug, prefix)}/publication.json";
        }

        public static string PublicContentLatestReleasePath(string slug, string prefix = null)
        {
            return $"{PublicContentPublicationParentPath(slug, prefix)}/latest-release.json";
        }

        public static string PublicContentReleasePath(string publicationSlug, string releaseSlug, string prefix = null)
        {
            return $"{PublicContentPublicationParentPath(publicationSlug, prefix)}/releases/{releaseSlug}.json";
        }

        public static string PrivateContentDataBlockParentPath(
            Guid publicationId,
            Guid releaseId)
        {
            return $"{PrivateContentReleaseParentPath(publicationId, releaseId)}/{PublicContentDataBlocksDirectory}";
        }

        public static string PublicContentDataBlockParentPath(
            string publicationSlug,
            string releaseSlug)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/{PublicContentDataBlocksDirectory}";
        }

        public static string PrivateContentDataBlockPath(
            Guid publicationId,
            Guid releaseId,
            Guid dataBlockId)
        {
            return $"{PrivateContentDataBlockParentPath(publicationId, releaseId)}/{dataBlockId}.json";
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
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/{PublicContentSubjectMetaDirectory}";
        }

        public static string PublicContentSubjectMetaPath(string publicationSlug, string releaseSlug, Guid subjectId)
        {
            return $"{PublicContentSubjectMetaParentPath(publicationSlug, releaseSlug)}/{subjectId}.json";
        }

        public static string PublicContentFastTrackResultsParentPath(string publicationSlug, string releaseSlug)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug)}/{PublicContentFastTrackResultsDirectory}";
        }

        public static string PublicContentFastTrackResultsPath(string publicationSlug, string releaseSlug, Guid fastTrackId)
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

        private static string AppendPathSeparator(string segment = null)
        {
            return segment == null ? "" : segment + "/";
        }
    }
}
