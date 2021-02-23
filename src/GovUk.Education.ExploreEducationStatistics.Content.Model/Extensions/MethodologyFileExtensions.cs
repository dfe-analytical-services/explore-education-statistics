namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class MethodologyFileExtensions
    {
        public static string Path(this MethodologyFile methodologyFile)
        {
            return methodologyFile.File.Path();
        }
    }
}
