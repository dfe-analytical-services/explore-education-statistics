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

        // TODO: Remove BlobInfo as parameter after EES-2343
        public static FileInfo ToPublicFileInfo(this ReleaseFile releaseFile, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = releaseFile.FileId,
                FileName = releaseFile.File.Filename,
                Name = releaseFile.Name ?? releaseFile.File.Filename,
                Summary = releaseFile.Summary,
                Size = blobInfo.Size,
                Type = releaseFile.File.Type,
            };
        }

        // TODO: Remove BlobInfo as parameter after EES-2343
        public static FileInfo ToFileInfo(this ReleaseFile releaseFile, BlobInfo blobInfo)
        {
            var info = releaseFile.ToPublicFileInfo(blobInfo);

            info.Created = releaseFile.File.Created;
            info.UserName = releaseFile.File.CreatedBy?.Email;

            return info;
        }

        // TODO: Remove after completion of EES-2343
        public static FileInfo ToFileInfoNotFound(this ReleaseFile releaseFile)
        {
            var fileInfo = releaseFile.ToPublicFileInfoNotFound();

            fileInfo.UserName = releaseFile.File.CreatedBy?.Email;
            fileInfo.Created = releaseFile.File.Created;

            return fileInfo;
        }

        // TODO: Remove after completion of EES-2343
        public static FileInfo ToPublicFileInfoNotFound(this ReleaseFile releaseFile)
        {
            return new FileInfo
            {
                Id = releaseFile.FileId,
                FileName = releaseFile.File.Filename,
                Name = releaseFile.Name ?? releaseFile.File.Filename,
                Summary = releaseFile.Summary,
                Size = FileInfo.UnknownSize,
                Type = releaseFile.File.Type,
            };
        }
    }
}