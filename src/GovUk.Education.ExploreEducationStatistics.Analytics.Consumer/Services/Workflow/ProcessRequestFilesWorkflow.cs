using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

public interface IProcessRequestFilesWorkflow
{
    Task Process(IWorkflowActor actor);
}

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
/// <param name="logger">
///     Logger from the associated Processor.
/// </param>
/// <param name="fileAccessor">
///     Optional <see cref="IFileAccessor"/> implementation. If not provided, defaults to an instance of
///     <see cref="FilesystemFileAccessor"/>.
/// </param>
/// <param name="dateTimeProvider">
///     Optional <see cref="DateTimeProvider"/>. If not provided, defaults to providing the current time.  
/// </param>
public class ProcessRequestFilesWorkflow(
    ILogger<ProcessRequestFilesWorkflow> logger,
    IFileAccessor? fileAccessor = null,
    DateTimeProvider? dateTimeProvider = null)
    : IProcessRequestFilesWorkflow
{
    private readonly IFileAccessor _fileAccessor = fileAccessor ?? new FilesystemFileAccessor();
    private readonly DateTimeProvider _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();

    public async Task Process(IWorkflowActor actor)
    {
        var processorName = actor.GetType().FullName;
        
        logger.LogInformation("{Processor} triggered", processorName);

        var sourceDirectory = actor.GetSourceDirectory();
        var reportsDirectory = actor.GetReportsDirectory();
        var processingDirectory = Path.Combine(sourceDirectory, "processing");
        var failuresDirectory = Path.Combine(sourceDirectory, "failures");
        
        if (!_fileAccessor.DirectoryExists(sourceDirectory))
        {
            logger.LogInformation("No requests for {Processor} to process", processorName);
            return;
        }

        var filesToProcess = _fileAccessor.ListFiles(sourceDirectory);

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No requests for {Processor} to process", processorName);
            return;
        }

        logger.LogInformation("Found {Count} requests for {Processor} to process", filesToProcess.Count,
            processorName);

        await using var duckDbConnection = new DuckDbConnection();
        duckDbConnection.Open();

        await actor.InitialiseDuckDb(duckDbConnection);

        _fileAccessor.CreateDirectory(processingDirectory);

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(sourceDirectory, file);
            var newPath = Path.Combine(processingDirectory, file);
            _fileAccessor.Move(originalPath, newPath);
        });

        var someFilesProcessedSuccessfully = false;
        
        foreach (var filename in filesToProcess)
        {
            try
            {
                await actor.ProcessSourceFile(
                    sourceFilePath: Path.Combine(processingDirectory, filename),
                    connection: duckDbConnection);

                someFilesProcessedSuccessfully = true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process request file {Filename}", filename);
                MoveBadFileToFailuresDirectory(
                    filename: filename,
                    processingDirectory: processingDirectory,
                    failuresDirectory: failuresDirectory);
            }
        }

        _fileAccessor.DeleteDirectory(processingDirectory);

        // If no files were successfully processed, there is no need to generate
        // reports, so exit early.
        if (!someFilesProcessedSuccessfully)
        {
            return;
        }
        
        _fileAccessor.CreateDirectory(reportsDirectory);
        
        var reportFilePathAndFilenamePrefix = Path.Combine(
            reportsDirectory,
            _dateTimeProvider.UtcNow.ToString("yyyyMMdd-HHmmss"));

        await actor.CreateParquetReports(
            reportsFolderPathAndFilenamePrefix: reportFilePathAndFilenamePrefix,
            connection: duckDbConnection);
    }

    private void MoveBadFileToFailuresDirectory(
        string filename,
        string processingDirectory,
        string failuresDirectory)
    {
        try
        {
            _fileAccessor.CreateDirectory(failuresDirectory);

            var fileSourcePath = Path.Combine(processingDirectory, filename);
            var fileDestPath = Path.Combine(failuresDirectory, filename);
            _fileAccessor.Move(fileSourcePath, fileDestPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to move bad file to failures directory {Filename}", filename);
        }
    }
}

/// <summary>
/// This interface represents a component that interacts with a file store
/// comprising directories and files.
///
/// It declares common filesystem interactions, mostly modelled from equivalent
/// calls from <see cref="Directory"/> and <see cref="File"/>.
/// </summary>
public interface IFileAccessor
{
    bool DirectoryExists(string directory);
    
    void CreateDirectory(string directory);
    
    void DeleteDirectory(string directory);
    
    IList<string> ListFiles(string directory);
    
    void Move(string sourcePath, string destinationPath);
}

/// <summary>
/// This default implementation of <see cref="IFileAccessor"/> interacts
/// directly with a standard filesystem, using <see cref="Directory"/>
/// and <see cref="File"/> to perform the work.
/// </summary>
internal class FilesystemFileAccessor : IFileAccessor
{
    public bool DirectoryExists(string directory)
    {
        return Directory.Exists(directory);
    }

    public void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
    }

    public void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, recursive: true);
    }

    public IList<string> ListFiles(string directory)
    {
        return Directory
            .GetFiles(directory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList(); 
    }

    public void Move(string sourcePath, string destinationPath)
    {
        Directory.Move(sourcePath, destinationPath);
    }
}

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
    /// <param name="sourceFilePath">
    /// The fully-qualified filepath for the source
    /// file to process.
    /// </param>
    /// <param name="connection">
    /// An open DuckDB connection that supports JSON reading and Parquet writing.
    /// </param>
    Task ProcessSourceFile(string sourceFilePath, DuckDbConnection connection);

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

