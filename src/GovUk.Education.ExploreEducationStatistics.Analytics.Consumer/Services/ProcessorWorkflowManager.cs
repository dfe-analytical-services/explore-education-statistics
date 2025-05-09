using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class ProcessorWorkflowManager(
    DuckDbConnection duckDbConnection,
    ICommonWorkflowRequestFileProcessor processor,
    ILogger<IRequestFileProcessor> logger)
{
    private readonly string _sourceDirectory = processor.SourceDirectory();
    private readonly string _processingDirectory = Path.Combine(processor.SourceDirectory(), "processing");
    private readonly string _failuresDirectory = Path.Combine(processor.SourceDirectory(), "failures");
    private readonly string _reportsDirectory = processor.ReportsDirectory();

    public Task ProcessWorkflow()
    {
        logger.LogInformation("{Processor} triggered", GetType().Name);
        
        if (!Directory.Exists(_sourceDirectory))
        {
            logger.LogInformation("No requests for {Processor} to process", GetType().Name);
            return Task.CompletedTask;
        }

        var filesToProcess = Directory
            .GetFiles(_sourceDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No requests for {Processor} to process", GetType().Name);
            return Task.CompletedTask;
        }

        logger.LogInformation("Found {Count} requests for {Processor} to process", filesToProcess.Count,
            GetType().Name);

        Directory.CreateDirectory(_processingDirectory);
        Directory.CreateDirectory(_reportsDirectory);

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(_sourceDirectory, file);
            var newPath = Path.Combine(_processingDirectory, file);
            File.Move(originalPath, newPath);
        });

        duckDbConnection.Open();

        duckDbConnection.ExecuteNonQuery("install json; load json");

        processor.InitialiseDuckDb(duckDbConnection);

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
                processor.ProcessSourceFile($"{_processingDirectory}/{filename}", duckDbConnection);
            }
            catch (DuckDBException e)
            {
                logger.LogError(e, "Failed to process request file {Filename}", filename);
                MoveBadFileToFailuresDirectory(filename);
            }
        }

        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var reportFilePath = Path.Combine(
            _reportsDirectory,
            $"{reportFilenamePrefix}_${processor.ReportsFilenameSuffix()}.parquet");

        processor.CreateParquetReport(reportFilePath, duckDbConnection);

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
