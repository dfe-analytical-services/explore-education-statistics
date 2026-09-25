#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Options;

// @ClaudeFix Update the bicep files: create the statistics data Azure File Share (shared with admin and data-api), mount it into the importer
// function app via `azureFileShares` in function-app.bicep, and set `DataFiles__BasePath` to the mount path.
public class DataFilesOptions
{
    public const string Section = "DataFiles";

    /// <summary>
    /// Base path where data set files (e.g. Parquet copies of data CSVs) are stored. This should be an absolute
    /// path in a non-local environment (i.e. where the File Share has been mounted), or a path relative to the
    /// project root in a local environment.
    /// </summary>
    public string BasePath { get; init; } = string.Empty;
}
