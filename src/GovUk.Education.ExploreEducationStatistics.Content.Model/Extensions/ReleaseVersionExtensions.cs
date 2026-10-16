#nullable enable
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class ReleaseVersionExtensions
{
    /// <summary>
    /// The storage blob path of the "All files" zip file for a release version.
    /// </summary>
    /// <param name="releaseVersion"></param>
    /// <returns></returns>
    public static string AllFilesZipPath(this ReleaseVersion releaseVersion)
    {
        return $"{FilesPath(releaseVersion.Id, AllFilesZip)}{releaseVersion.AllFilesZipFileName()}";
    }

    /// <summary>
    /// The versioned storage path used for newly generated "All files" zip files.
    /// Changing the format version creates a new blob and a new edge-cache key.
    /// </summary>
    public static string AllFilesZipPath(this ReleaseVersion releaseVersion, int formatVersion)
    {
        // Version 1 keeps the existing path so rollout does not trigger a mass cold-cache rebuild.
        return formatVersion == 1
            ? releaseVersion.AllFilesZipPath()
            : $"{FilesPath(releaseVersion.Id, AllFilesZip)}v{formatVersion}/{releaseVersion.AllFilesZipFileName()}";
    }

    public static string AllFilesZipFileName(this ReleaseVersion releaseVersion)
    {
        if (releaseVersion.Release?.Publication == null)
        {
            throw new ArgumentException(
                "ReleaseVersion must be hydrated with Publication to create All Files zip file name"
            );
        }

        return $"{releaseVersion.Release.Publication.Slug}_{releaseVersion.Release.Slug}.zip";
    }
}
