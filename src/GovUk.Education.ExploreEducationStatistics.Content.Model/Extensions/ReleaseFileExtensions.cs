#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

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

        public static FileInfo ToPublicFileInfo(this ReleaseFile releaseFile)
        {
            return new FileInfo
            {
                Id = releaseFile.FileId,
                FileName = releaseFile.File.Filename,
                Name = releaseFile.Name ?? string.Empty,
                Summary = releaseFile.Summary,
                Size = releaseFile.File.DisplaySize(),
                Type = releaseFile.File.Type,
            };
        }

        public static FileInfo ToFileInfo(this ReleaseFile releaseFile)
        {
            var info = releaseFile.ToPublicFileInfo();

            info.Created = releaseFile.File.Created;
            info.UserName = releaseFile.File.CreatedBy?.Email;

            return info;
        }
    }
}
