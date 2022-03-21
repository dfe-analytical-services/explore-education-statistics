#nullable enable
using System;
using System.Linq;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class ReleaseExtensions
    {
        /// <summary>
        /// The storage blob path of the "All files" zip file on a Release.
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        public static string AllFilesZipPath(this Release release)
        {
            return $"{FilesPath(release.Id, AllFilesZip)}{release.AllFilesZipFileName()}";
        }

        public static string AllFilesZipFileName(this Release release)
        {
            if (release.Publication == null)
            {
                throw new ArgumentException("Release must be hydrated with Publication to create All Files zip file name");
            }

            return $"{release.Publication.Slug}_{release.Slug}.zip";
        }

        /// <summary>
        /// Determines whether a Release is the latest published version of a Release within its Publication, i.e. no newer published amendments of that Release exist.
        /// </summary>
        /// <param name="release">The Release to test</param>
        /// <returns>True if the Release is the latest published version of a Release</returns>
        public static bool IsLatestPublishedVersionOfRelease(this Release release)
        {
            if (release.Publication?.Releases == null || !release.Publication.Releases.Any())
            {
                throw new ArgumentException(
                    "Release must be hydrated with Publications Releases to test the latest published version");
            }

            return
                // Release itself must be live
                release.Live
                // It must also be the latest version unless the later version is a draft
                && !release.Publication.Releases.Any(r =>
                    r.Live
                    && r.PreviousVersionId == release.Id
                    && r.Id != release.Id);
        }
    }
}
