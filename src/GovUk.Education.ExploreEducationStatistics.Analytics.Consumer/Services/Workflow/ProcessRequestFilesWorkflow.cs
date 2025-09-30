using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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
/// <param name="logger">
///     Logger from the associated Processor.
/// </param>
/// <param name="fileAccessor">
///     Abstracted file system handler.
/// </param>
/// <param name="dateTimeProvider">
///     Abstracted date and time provider  
/// </param>
public class ProcessRequestFilesWorkflow(
    ILogger<ProcessRequestFilesWorkflow> logger,
    IFileAccessor fileAccessor,
    DateTimeProvider dateTimeProvider,
    Func<string>? temporaryProcessingFolderNameGenerator = null,
    int batchSize = 100)
    : IProcessRequestFilesWorkflow
{
    public async Task Process(IWorkflowActor actor)
    {
        var processorName = actor.GetType().FullName;

        logger.LogInformation("{Processor} triggered", processorName);

        var sourceDirectory = actor.GetSourceDirectory();
        var reportsDirectory = actor.GetReportsDirectory();
        var baseProcessingDirectory = Path.Combine(sourceDirectory, "processing");

        if (!fileAccessor.DirectoryExists(sourceDirectory))
        {
            logger.LogInformation("No requests for {Processor} to process", processorName);
            return;
        }

        var filesToProcess = fileAccessor
            .ListFiles(sourceDirectory)
            .Order()
            .ToList();

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

        var temporaryProcessingDirectoryName =
            temporaryProcessingFolderNameGenerator?.Invoke() ?? Guid.NewGuid().ToString();

        var temporaryProcessingDirectory = Path.Combine(
            baseProcessingDirectory,
            temporaryProcessingDirectoryName);

        var fileBatches = filesToProcess
            .Batch(batchSize)
            .Select(batch => batch.ToList())
            .ToList();

        // For each batch of files to process, move them into their own folder
        // under an overarching temporary processing folder.
        fileBatches.ForEach((batchFilenames, index) =>
        {
            var batchNumber = index + 1;

            var batchDirectory = GetBatchDirectoryPath(
                temporaryProcessingDirectory: temporaryProcessingDirectory,
                batchNumber: batchNumber);

            MoveFileBatch(
                filenames: batchFilenames,
                sourceDirectory: sourceDirectory,
                targetDirectory: batchDirectory,
                createTargetDirectory: true);
        });

        var batchProcessingResults = Enumerable
            .Range(start: 1, count: fileBatches.Count)
            .SelectAsync(async batchNumber => await ProcessFileBatch(
                actor: actor,
                sourceDirectory: sourceDirectory,
                temporaryProcessingDirectory: temporaryProcessingDirectory,
                temporaryProcessingDirectoryName: temporaryProcessingDirectoryName,
                batchNumber: batchNumber,
                // ReSharper disable once AccessToDisposedClosure
                duckDbConnection: duckDbConnection));

        await batchProcessingResults;

        var someFileBatchesProcessedSuccessfully = batchProcessingResults
            .Result
            .Any(result => result);

        fileAccessor.DeleteDirectory(temporaryProcessingDirectory);

        // If no files were successfully processed, there is no need to generate
        // reports, so exit early.
        if (!someFileBatchesProcessedSuccessfully)
        {
            return;
        }

        fileAccessor.CreateDirectory(reportsDirectory);

        var reportFilePathAndFilenamePrefix = Path.Combine(
            reportsDirectory,
            dateTimeProvider.UtcNow.ToString("yyyyMMdd-HHmmss"));

        await actor.CreateParquetReports(
            reportsFolderPathAndFilenamePrefix: reportFilePathAndFilenamePrefix,
            connection: duckDbConnection);
    }

    private async Task<bool> ProcessFileBatch(
        IWorkflowActor actor,
        string sourceDirectory,
        string temporaryProcessingDirectory,
        string temporaryProcessingDirectoryName,
        int batchNumber,
        DuckDbConnection duckDbConnection)
    {
        var batchDirectory = GetBatchDirectoryPath(
            temporaryProcessingDirectory: temporaryProcessingDirectory,
            batchNumber: batchNumber);

        try
        {
            await actor.ProcessSourceFiles(
                sourceFilesDirectory: Path.Combine(batchDirectory, "*"),
                connection: duckDbConnection);

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process request file batch");

            var failuresDirectory = Path.Combine(
                sourceDirectory,
                "failures",
                temporaryProcessingDirectoryName);

            fileAccessor.CreateDirectory(failuresDirectory);

            MoveBadBatchDirectoryToFailuresDirectory(
                batchDirectory: batchDirectory,
                failuresDirectory: failuresDirectory,
                batchNumber: batchNumber);

            return false;
        }
    }

    private static string GetBatchDirectoryPath(
        string temporaryProcessingDirectory,
        int batchNumber)
    {
        return Path.Combine(
            temporaryProcessingDirectory,
            batchNumber.ToString());
    }

    private void MoveBadBatchDirectoryToFailuresDirectory(
        string batchDirectory,
        string failuresDirectory,
        int batchNumber)
    {
        try
        {
            fileAccessor.Move(
                sourcePath: batchDirectory,
                destinationPath: Path.Combine(failuresDirectory, batchNumber.ToString()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to move bad file batch to failures directory");
        }
    }

    private void MoveFileBatch(
        IEnumerable<string> filenames,
        string sourceDirectory,
        string targetDirectory,
        bool createTargetDirectory = true)
    {
        if (createTargetDirectory)
        {
            fileAccessor.CreateDirectory(targetDirectory);
        }

        Parallel.ForEach(filenames, filename =>
        {
            var fileSourcePath = Path.Combine(sourceDirectory, filename);
            var fileDestPath = Path.Combine(targetDirectory, filename);
            fileAccessor.Move(fileSourcePath, fileDestPath);
        });
    }
}
