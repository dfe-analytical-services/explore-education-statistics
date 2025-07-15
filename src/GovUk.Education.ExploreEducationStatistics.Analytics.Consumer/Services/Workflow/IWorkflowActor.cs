using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

/// <summary>
/// This class represents an actor who is guided through the workflow
/// as implemented in <see cref="ProcessRequestFilesWorkflow"/>.
///
/// This class is responsible for implementing steps that
/// are carried out during the workflow that are specific to individual
/// use cases e.g. collecting data about and generating Parquet report
/// files for Public API queries.
/// </summary>
public interface IWorkflowActor
{
    string GetSourceDirectory();
    
    string GetReportsDirectory();

    /// <summary>
    /// Create any initial tables and state needed for collecting information
    /// from source files.
    /// </summary>
    Task InitialiseDuckDb(DuckDbConnection connection);

    /// <summary>
    /// Given a source file, process it, generally by reading it into DuckDb
    /// into tables set up at the beginning of the workflow.
    /// </summary>
    /// <param name="sourceFilesDirectory"></param>
    /// <param name="connection">
    ///     An open DuckDB connection that supports JSON reading and Parquet writing.
    /// </param>
    Task ProcessSourceFiles(string sourceFilesDirectory, DuckDbConnection connection);

    /// <summary>
    /// Create one or more Parquet report files based on the source files that
    /// have been read.
    /// </summary>
    /// <param name="reportsFolderPathAndFilenamePrefix">
    /// The fully-qualified folder path and filename prefix for any reports being
    /// generated. In addition to a name suffix to identify each report being generated
    /// here, '.parquet' will also need to be appended to any report filenames.
    /// </param>
    /// <param name="connection">
    /// An open DuckDB connection that supports JSON reading and Parquet writing.
    /// </param>
    Task CreateParquetReports(string reportsFolderPathAndFilenamePrefix, DuckDbConnection connection);
}