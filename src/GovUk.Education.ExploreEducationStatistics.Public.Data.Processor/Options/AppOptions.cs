namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;

public class AppOptions
{
    public const string Section = "App";

    public required string PrivateStorageConnectionString { get; set; }

    /// <summary>
    /// Batch size to use when inserting location option meta rows, location option meta link rows,
    /// and filter option meta link rows into the public data db.
    /// </summary>
    public required int MetaInsertBatchSize { get; set; }

    public bool EnableThemeDeletion { get; set; }
}
