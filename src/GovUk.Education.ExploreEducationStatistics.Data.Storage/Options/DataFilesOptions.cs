#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Options;

public class DataFilesOptions
{
    public const string Section = "DataFiles";

    /// <summary>
    /// Base path where data set files (e.g. parquet files) are stored. This should be an absolute path
    /// in a non-local environment (i.e. where the File Share has been mounted), or a path relative to
    /// the project root in a local environment.
    /// </summary>
    public required string BasePath { get; init; }
}
