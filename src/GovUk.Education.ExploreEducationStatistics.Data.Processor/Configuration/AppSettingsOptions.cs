#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Configuration;

public class AppSettingsOptions
{
    public const string Section = "AppSettings";

    public string PrivateStorageConnectionString { get; init; } = null!;

    public int RowsPerBatch { get; init; }
}
