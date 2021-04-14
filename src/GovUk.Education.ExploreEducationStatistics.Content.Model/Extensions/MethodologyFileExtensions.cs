using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class MethodologyFileExtensions
    {
        public static string Path(this MethodologyFile methodologyFile)
        {
            return methodologyFile.File.Path();
        }

        public static FileInfo ToFileInfo(this MethodologyFile methodologyFile, BlobInfo blobInfo)
        {
            return new FileInfo
            {
                Id = methodologyFile.File.Id,
                FileName = methodologyFile.File.Filename,
                Name = null,
                Size = blobInfo.Size,
                Type = methodologyFile.File.Type
            };
        }

    }
}
