using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStoragePathUtils
    {
        public static string PublicContentStagingPath()
        {
            return "staging";
        }

        private static string PublicContentFastTrackPath(string prefix = null)
        {
            return $"{AppendPathSeparator(prefix)}fast-track";
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

        public static string PublicContentPublicationParentPath(string slug, string prefix = null)
        {
            return $"{PublicContentPublicationsPath(prefix)}/{slug}";
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

        public static string PublicContentDataBlockPath(
            string publicationSlug,
            string releaseSlug,
            Guid dataBlockId,
            string prefix = null)
        {
            return $"{PublicContentReleaseParentPath(publicationSlug, releaseSlug, prefix)}/data-blocks/{dataBlockId}.json";
        }

        public static string PublicContentSubjectMetaPath(Guid releaseId)
        {
            return $"subject-meta/{releaseId}";
        }
        
        public static string PublicContentSubjectMetaPath(Guid releaseId, Guid subjectId)
        {
            return $"{PublicContentSubjectMetaPath(releaseId)}/{subjectId}.json";
        }
        
        public static string PublicContentFastTrackResultsPath(Guid releaseId)
        {
            return $"fast-track-results/{releaseId}";
        }

        public static string PublicContentFastTrackResultsPath(Guid releaseId, Guid fastTrackId)
        {
            return $"{PublicContentFastTrackResultsPath(releaseId)}/{fastTrackId}.json";
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
