namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public static class ImporterExtensions
    {
        public static string NullIfWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}