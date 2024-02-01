#nullable enable
using System;
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
    }
}
