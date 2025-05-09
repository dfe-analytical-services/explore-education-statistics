using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

/// <summary>
/// This class represents a common workflow that a number of IRequestFileProcessor
/// implementations use in order to generate their Parquet report files.
///
/// As a guide, these steps involve:
///
/// * Checking to see if there is work to do.
/// * Initialising a set of data collection tables in DuckDb.
/// * Processing source files into the data collection tables.
/// * Generating Parquet reports from the collected data.
/// * Any error handling needed during this process.
///
/// This workflow guides an implementation of IWorkflowActor
/// through these common steps, using the actor to perform
/// implementation-specific steps along the way. 
/// 
/// </summary>
/// <param name="processorName">
///     Class name of the <see cref="IRequestFileProcessor" /> using this workflow.
/// </param>
/// <param name="sourceDirectory">
///     Folder in which to find the source files to read in this workflow.
/// </param>
/// <param name="reportsDirectory">
///     Folder in which to generate the Parquet report files in this workflow.
/// </param>
/// <param name="actor">
///     The <see cref="IWorkflowActor" /> implementation being guided through the workflow.
/// </param>
public class ProcessRequestFilesWorkflow(
    string processorName,
    string sourceDirectory,
    string reportsDirectory,
    IWorkflowActor actor,
    ILogger<IRequestFileProcessor> logger)
{
    private readonly string _processingDirectory = Path.Combine(sourceDirectory, "processing");
    private readonly string _failuresDirectory = Path.Combine(sourceDirectory, "failures");

    public Task Process()
    {
        logger.LogInformation("{Processor} triggered", processorName);
        
        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No requests for {Processor} to process", processorName);
            return Task.CompletedTask;
        }

        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No requests for {Processor} to process", processorName);
            return Task.CompletedTask;
        }

        logger.LogInformation("Found {Count} requests for {Processor} to process", filesToProcess.Count,
            processorName);

        Directory.CreateDirectory(_processingDirectory);
        Directory.CreateDirectory(reportsDirectory);

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(sourceDirectory, file);
            var newPath = Path.Combine(_processingDirectory, file);
            File.Move(originalPath, newPath);
        });

        using var duckDbConnection = new DuckDbConnection();

        duckDbConnection.Open();

        duckDbConnection.ExecuteNonQuery("install json; load json");

        actor.InitialiseDuckDb(duckDbConnection);

        // We fetch the files again in case there are files leftover in the processing dir
        // from a previous function run.
        var filesReadyForProcessing = Directory
            .GetFiles(_processingDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        foreach (var filename in filesReadyForProcessing)
        {
            try
            {
                actor.ProcessSourceFile(
                    sourceFilePath: $"{_processingDirectory}/{filename}",
                    connection: duckDbConnection);
            }
            catch (DuckDBException e)
            {
                logger.LogError(e, "Failed to process request file {Filename}", filename);
                MoveBadFileToFailuresDirectory(filename);
            }
        }

        var reportFilePathAndFilenamePrefix = Path.Combine(
            reportsDirectory,
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));

        actor.CreateParquetReports(
            reportsFilePathAndFilenamePrefix: reportFilePathAndFilenamePrefix,
            connection: duckDbConnection);

        Directory.Delete(_processingDirectory, recursive: true);

        return Task.CompletedTask;
    }

    private void MoveBadFileToFailuresDirectory(string filename)
    {
        try
        {
            Directory.CreateDirectory(_failuresDirectory);

            var fileSourcePath = Path.Combine(_processingDirectory, filename);
            var fileDestPath = Path.Combine(_failuresDirectory, filename);
            File.Move(fileSourcePath, fileDestPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to move bad file to failures directory {Filename}", filename);
        }
    }
}

/// <summary>
/// This class represents an actor who is guided through the workflow
/// as implemented in <see cref="ProcessRequestFilesWorkflow"/>.
///
/// This class is responsible for implementing steps that
/// are carried out during the workflow that are specific to indicidual
/// use cases e.g. collecting data about and generating Parquet report
/// files for Public API queries.
/// </summary>
public interface IWorkflowActor
{
    /// <summary>
    /// Create any initial tables and state needed for collecting information
    /// from source files.
    /// </summary>
    void InitialiseDuckDb(DuckDbConnection connection);

    /// <summary>
    /// Given a source file, process it, generally by reading it into DuckDb
    /// into tables set up at the beginning of the workflow.
    /// </summary>
    /// <param name="sourceFilePath">The fully-qualified filepath for the source
    /// file to process.</param>
    void ProcessSourceFile(string sourceFilePath, DuckDbConnection connection);

    /// <summary>
    /// Create one or more Parquet report files based on the source files that
    /// have been read.
    /// </summary>
    /// <param name="reportsFilePathAndFilenamePrefix">
    /// The fully-qualified folder path and filename prefix for any reports being
    /// generated. In addition to a name suffix to identify each report being generated
    /// here, '.parquet' will also need to be appended to any report filenames.
    /// </param>
    void CreateParquetReports(string reportsFilePathAndFilenamePrefix, DuckDbConnection connection);
}

