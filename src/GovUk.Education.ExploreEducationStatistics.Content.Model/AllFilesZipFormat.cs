namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public static class AllFilesZipFormat
{
    /// <summary>
    /// Increment this version whenever the contents or structure of the all-files ZIP changes.
    /// This creates a new Blob path and AFD cache key, preventing the previous ZIP format from being served.
    /// </summary>
    public const int CurrentVersion = 1;
}
