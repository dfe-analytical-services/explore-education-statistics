using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class ReleaseFileExtensions
    {
        public static string BatchesPath(this ReleaseFile releaseFile)
        {
            return releaseFile.File.BatchesPath();
        }

        public static string Path(this ReleaseFile releaseFile)
        {
            return releaseFile.File.Path();
        }

        public static string PublicPath(this ReleaseFile releaseFile)
        {
            return releaseFile.File.PublicPath(releaseFile.Release);
        }

        public static FileInfo ToFileInfo(this ReleaseFile releaseFile, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = releaseFile.FileId,
                FileName = releaseFile.File.Filename,
                Name = releaseFile.Name ?? releaseFile.File.Filename,
                Size = blobInfo.Size,
                Type = releaseFile.File.Type
            };
        }
    }
}
