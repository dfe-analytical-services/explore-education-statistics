#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Options;

public class AppOptions
{
    public const string Section = "App";

    public string PrivateStorageConnectionString { get; init; } = null!;

    public int RowsPerBatch { get; init; }
}
