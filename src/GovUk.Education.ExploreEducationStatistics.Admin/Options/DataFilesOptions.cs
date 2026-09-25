#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

// @ClaudeFix Update the bicep files: mount the statistics data Azure File Share (shared with data-api and the importer) into the admin app
// service via `azureFileShares` in app-service.bicep, and set `DataFiles__BasePath` to the mount path.
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
