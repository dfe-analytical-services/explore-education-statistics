using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;

public class ParquetFilesOptions
{
    public static readonly string Section = "ParquetFiles";

    /// <summary>
    /// Base path where Parquet files are stored. This should be an absolute path
    /// in a non-local environment (i.e. where the File Share has been mounted),
    /// or a relative path in a local environment.
    /// </summary>
    public required string BasePath { get; init; }
}
