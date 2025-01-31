namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;

public class DataFilesOptions
{
    public const string Section = "DataFiles";

    /// <summary>
    /// Base path where data files are stored. This should be an absolute path
    /// in a non-local environment (i.e. where the File Share has been mounted),
    /// or a relative path in a local environment.
    /// </summary>
    public required string BasePath { get; init; }
}
