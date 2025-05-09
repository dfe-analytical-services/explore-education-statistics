using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

public class ProcessRequestFilesWorkflow(
    string processorName,
    string sourceDirectory,
    string reportsDirectory,
    Action<InitialiseDuckDbContext> initialiseAction,
    Action<ProcessSourceFileContext> processSourceFileAction,
    Action<CreateParquetReportsContext> createParquetReportsAction,
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

        initialiseAction(new InitialiseDuckDbContext(duckDbConnection));

        // We fetch the files again in case there are files leftover in the processing dir from a previous function run
        var filesReadyForProcessing = Directory
            .GetFiles(_processingDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        foreach (var filename in filesReadyForProcessing)
        {
            try
            {
                processSourceFileAction(new ProcessSourceFileContext(
                    SourceFilePath: $"{_processingDirectory}/{filename}",
                    Connection: duckDbConnection));
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

        createParquetReportsAction(new CreateParquetReportsContext(
            ReportsFilePathAndFilenamePrefix: reportFilePathAndFilenamePrefix,
            Connection: duckDbConnection));

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

public record InitialiseDuckDbContext(DuckDbConnection Connection);

public record ProcessSourceFileContext(string SourceFilePath, DuckDbConnection Connection);

public record CreateParquetReportsContext(string ReportsFilePathAndFilenamePrefix, DuckDbConnection Connection);
