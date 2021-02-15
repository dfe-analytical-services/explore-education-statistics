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
    }
}
