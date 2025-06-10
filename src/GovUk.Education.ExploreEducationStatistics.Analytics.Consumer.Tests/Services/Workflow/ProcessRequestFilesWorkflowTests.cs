using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow;

public abstract class ProcessRequestFilesWorkflowTests
{
    private static readonly string ProcessingFolder = Path.Combine(SourceFolder, "processing");
    
    public const string SourceFolder = "source";
    public const string ReportsFolder = "reports";
    public static readonly string TemporaryProcessingDirectory = Path.Combine(ProcessingFolder, "temp-processing-folder");
    public static readonly string FailuresFolder = Path.Combine(SourceFolder, "failures", "temp-processing-folder");
    public const string ReportsFilenamePrefix = "20220316-120102";

    public class ProcessTests : ProcessRequestFilesWorkflowTests
    {
        private readonly FileAccessorMockBuilder _fileAccessorMockBuilder;
        private readonly WorkflowActorMockBuilder _workflowActorMockBuilder;
        private readonly FileAccessorMockBuilder.Asserter _fileAccessorAsserter;
        private readonly WorkflowActorMockBuilder.Asserter _workflowActorAsserter;

        public ProcessTests()
        {
            _fileAccessorMockBuilder = new FileAccessorMockBuilder();
            _workflowActorMockBuilder = new WorkflowActorMockBuilder();
            _fileAccessorAsserter = _fileAccessorMockBuilder.Assert;
            _workflowActorAsserter = _workflowActorMockBuilder.Assert;
        }

        [Fact]
        public async Task NoSourceFolder_NoReportsProduced()
        {
            var fileAccessor = _fileAccessorMockBuilder
                .WhereDirectoryDoesNotExist(SourceFolder)
                .Build();

            var workflow = BuildWorkflow(fileAccessor: fileAccessor);

            await workflow.Process(_workflowActorMockBuilder.Build());

            // Ensure no reports were generated or reports folder created
            // because no source folder existed.
            _fileAccessorAsserter.DirectoryExistsCalledFor(SourceFolder);
        }

        [Fact]
        public async Task NoSourceFilesToConsume_NoReportsProduced()
        {
            var fileAccessor = _fileAccessorMockBuilder
                .WhereDirectoryExists(SourceFolder)
                .WhereFileListForDirectoryIs(SourceFolder, [])
                .Build();

            var workflow = BuildWorkflow(fileAccessor: fileAccessor);

            await workflow.Process(_workflowActorMockBuilder.Build());

            // Ensure no reports were generated or reports folder created
            // because no source files existed.
            _fileAccessorAsserter
                .DirectoryExistsCalledFor(SourceFolder)
                .FileListForDirectoryCalledFor(SourceFolder);
        }

        [Fact]
        public async Task SourceFilesExist_ReportsProduced()
        {
            var batchSize = 3;
            string[] sourceFiles = ["file5", "file2", "file4", "file3", "file1"];
            string[] batch1Files = ["file1", "file2", "file3"];
            string[] batch2Files = ["file4", "file5"];

            var batch1ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "1");
            var batch2ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "2");

            // The workflow checks for the existence of any source files before continuing.
            _fileAccessorMockBuilder.WhereSourceFilesExist(sourceFiles);

            // The workflow calls the implementation to set up its source tables.
            _workflowActorMockBuilder.WhereDuckDbInitialisedSuccessfully();

            // The workflow sets up a temporary processing folder within the root "processing" folder
            // and moves all current files into it.  It will be files from within this temporary folder
            // that will be processed by this run of the function.
            _fileAccessorMockBuilder.WhereTemporaryProcessingFolderIsSetUpForThisRun(sourceFiles);
            
            List<(string, string[])> batchProcessingFoldersAndFiles = 
            [
                (batch1ProcessingDirectory, batch1Files),
                (batch2ProcessingDirectory, batch2Files),
            ];

            // For each batch of files to process, a separate batch folder is created and the files within
            // processed by the implementation.
            batchProcessingFoldersAndFiles.ForEach(folderAndFiles =>
            {
                var (batchFolder, batchOfFiles) = folderAndFiles;
                
                _fileAccessorMockBuilder
                    .WhereBatchOfFilesIsPreparedForProcessing(
                        batchProcessingDirectory: batchFolder,
                        sourceFiles: batchOfFiles);

                _workflowActorMockBuilder
                    .WhereSourceFileBatchIsProcessedSuccessfully(
                        batchProcessingFolder: batchFolder);
            });

            // The temporary processing folder that was set up for this run of the function is removed
            // after source files are read in, and because we have had at least one successful batch
            // processed, it prepares to generate a report by firstly creating the reports folder if
            // it doesn't yet exist.
            _fileAccessorMockBuilder
                .WhereTemporaryProcessingDirectoryIsRemovedAfterProcessing()
                .WhereReportsDirectoryIsCreatedForReportGeneration();

            // The workflow calls the implementation to generate its report.
            _workflowActorMockBuilder.WhereReportGeneratedSuccessfully();

            var workflow = BuildWorkflow(
                fileAccessor: _fileAccessorMockBuilder.Build(),
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")),
                batchSize: batchSize);

            await workflow.Process(_workflowActorMockBuilder.Build());

            _fileAccessorAsserter.SourceFilesChecked();
            _workflowActorAsserter.InitialiseDuckDbCalled();
            _fileAccessorAsserter.TemporaryProcessingFolderWasSetUpForThisRun(sourceFiles: sourceFiles);

            batchProcessingFoldersAndFiles.ForEach(folderAndFiles =>
            {
                var (batchFolder, batchOfFiles) = folderAndFiles;
                
                _fileAccessorAsserter
                    .BatchOfFilesWasPreparedForProcessing(
                        batchProcessingDirectory: batchFolder,
                        sourceFiles: batchOfFiles);

                _workflowActorAsserter
                    .ProcessSourceFileBatchCalledFor(batchFolder);
            });

            _fileAccessorAsserter
                .TemporaryProcessingDirectoryWasRemovedAfterProcessing()
                .ReportsDirectoryWasCreatedForReportGeneration();

            _workflowActorAsserter.CreateReportWasCalled();
        }

        [Fact]
        public async Task ErrorInitialisingDuckDb_ExceptionThrownAndExitedEarly()
        {
            string[] sourceFiles = ["file1", "file2"];
            
            _fileAccessorMockBuilder
                .WhereSourceFilesExist(sourceFiles)
                .WhereTemporaryProcessingFolderIsSetUpForThisRun(sourceFiles);

            _workflowActorMockBuilder.WhereDuckDbInitialisedWithErrors();

            var workflow = BuildWorkflow(
                fileAccessor: _fileAccessorMockBuilder.Build());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                workflow.Process(_workflowActorMockBuilder.Build()));

            _fileAccessorAsserter.SourceFilesChecked();

            _workflowActorAsserter.InitialiseDuckDbCalled();
        }

        [Fact]
        public async Task ErrorsProcessingSomeFileBatches_MovedToFailuresFolderButReportGenerated()
        {
            var batchSize = 2;
            string[] sourceFiles = ["file1", "file3", "file2", "file4", "file5"];
            string[] batch1Files = ["file1", "file2"];
            string[] batch2Files = ["file3", "file4"];
            string[] batch3Files = ["file5"];

            var batch1ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "1");
            var batch2ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "2");
            var batch3ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "3");

            // The workflow checks for the existence of any source files before continuing.
            _fileAccessorMockBuilder.WhereSourceFilesExist(sourceFiles);

            // The workflow calls the implementation to set up its source tables.
            _workflowActorMockBuilder.WhereDuckDbInitialisedSuccessfully();

            // The workflow sets up a temporary processing folder within the root "processing" folder
            // and moves all current files into it.  It will be files from within this temporary folder
            // that will be processed by this run of the function.
            _fileAccessorMockBuilder
                .WhereTemporaryProcessingFolderIsSetUpForThisRun(sourceFiles);
            
            // Batch 1 is processed successfully.
            _fileAccessorMockBuilder
                .WhereBatchOfFilesIsPreparedForProcessing(
                    batchProcessingDirectory: batch1ProcessingDirectory,
                    sourceFiles: batch1Files);

            _workflowActorMockBuilder
                .WhereSourceFileBatchIsProcessedSuccessfully(
                    batchProcessingFolder: batch1ProcessingDirectory);
            
            // Batch 2 is processed with failures.
            _fileAccessorMockBuilder
                .WhereBatchOfFilesIsPreparedForProcessing(
                    batchProcessingDirectory: batch2ProcessingDirectory,
                    sourceFiles: batch2Files);

            _workflowActorMockBuilder
                .WhereSourceFilesAreProcessedWithErrors(
                    batchProcessingFolder: batch2ProcessingDirectory);

            _fileAccessorMockBuilder
                .WhereBatchOfFilesFailsProcessing(
                    batchProcessingDirectory: batch2ProcessingDirectory,
                    batchNumber: 2);
            
            // Batch 3 is processed successfully.
            _fileAccessorMockBuilder
                .WhereBatchOfFilesIsPreparedForProcessing(
                    batchProcessingDirectory: batch3ProcessingDirectory,
                    sourceFiles: batch3Files);

            _workflowActorMockBuilder
                .WhereSourceFileBatchIsProcessedSuccessfully(
                    batchProcessingFolder: batch3ProcessingDirectory);

            // The temporary processing folder that was set up for this run of the function is removed
            // after source files are read in, and because we have had at least one successful batch
            // processed, it prepares to generate a report by firstly creating the reports folder if
            // it doesn't yet exist.
            _fileAccessorMockBuilder
                .WhereTemporaryProcessingDirectoryIsRemovedAfterProcessing()
                .WhereReportsDirectoryIsCreatedForReportGeneration();

            // The workflow calls the implementation to generate its report because it had at least 1
            // batch of files that succeeded.
            _workflowActorMockBuilder.WhereReportGeneratedSuccessfully();

            var workflow = BuildWorkflow(
                fileAccessor: _fileAccessorMockBuilder.Build(),
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")),
                batchSize: batchSize);

            await workflow.Process(_workflowActorMockBuilder.Build());

            _fileAccessorAsserter.SourceFilesChecked();
            _workflowActorAsserter.InitialiseDuckDbCalled();

            _fileAccessorAsserter
                .TemporaryProcessingFolderWasSetUpForThisRun(sourceFiles: sourceFiles)
                .BatchOfFilesWasPreparedForProcessing(
                    batchProcessingDirectory: batch1ProcessingDirectory,
                    sourceFiles: batch1Files)
                .BatchOfFilesWasPreparedForProcessing(
                    batchProcessingDirectory: batch2ProcessingDirectory,
                    sourceFiles: batch2Files)
                .BatchOfFilesWasPreparedForProcessing(
                    batchProcessingDirectory: batch3ProcessingDirectory,
                    sourceFiles: batch3Files);

            _workflowActorAsserter
                .ProcessSourceFileBatchCalledFor(batch1ProcessingDirectory)
                .ProcessSourceFileBatchCalledFor(batch2ProcessingDirectory)
                .ProcessSourceFileBatchCalledFor(batch3ProcessingDirectory);

            _fileAccessorAsserter
                .FailedSourceFileBatchMovedToFailureDirectory(
                    batchProcessingDirectory: batch2ProcessingDirectory,
                    batchNumber: 2);
                
            _fileAccessorAsserter
                .TemporaryProcessingDirectoryWasRemovedAfterProcessing()
                .ReportsDirectoryWasCreatedForReportGeneration();

            _workflowActorAsserter.CreateReportWasCalled();
        }

        [Fact]
        public async Task ErrorsProcessingAllFiles_MovedToFailuresFolderAndNoReportGenerated()
        {
            var batchSize = 2;
            string[] sourceFiles = ["file1", "file3", "file2"];
            string[] batch1Files = ["file1", "file2"];
            string[] batch2Files = ["file3"];

            var batch1ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "1");
            var batch2ProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "2");

            // The workflow checks for the existence of any source files before continuing.
            _fileAccessorMockBuilder.WhereSourceFilesExist(sourceFiles);

            // The workflow calls the implementation to set up its source tables.
            _workflowActorMockBuilder.WhereDuckDbInitialisedSuccessfully();

            // The workflow sets up a temporary processing folder within the root "processing" folder
            // and moves all current files into it.  It will be files from within this temporary folder
            // that will be processed by this run of the function.
            _fileAccessorMockBuilder
                .WhereTemporaryProcessingFolderIsSetUpForThisRun(sourceFiles);
            
            List<(string, string[], int)> batchProcessingFoldersAndFiles = 
            [
                (batch1ProcessingDirectory, batch1Files, 1),
                (batch2ProcessingDirectory, batch2Files, 2),
            ];
            
            // All batches fail processing.
            batchProcessingFoldersAndFiles.ForEach(folderAndFiles =>
            {
                var (batchFolder, batchOfFiles, batchNumber) = folderAndFiles;
                
                _fileAccessorMockBuilder
                    .WhereBatchOfFilesIsPreparedForProcessing(
                        batchProcessingDirectory: batchFolder,
                        sourceFiles: batchOfFiles);

                _workflowActorMockBuilder
                    .WhereSourceFilesAreProcessedWithErrors(
                        batchProcessingFolder: batchFolder);

                _fileAccessorMockBuilder
                    .WhereBatchOfFilesFailsProcessing(
                        batchProcessingDirectory: batchFolder,
                        batchNumber: batchNumber);
            });

            // The temporary processing folder that was set up for this run of the function is removed
            // after source files are read in.
            //
            // Because we didn't have any successful file batches processed though, a reports folder is
            // not generated. 
            _fileAccessorMockBuilder.WhereTemporaryProcessingDirectoryIsRemovedAfterProcessing();

            var workflow = BuildWorkflow(
                fileAccessor: _fileAccessorMockBuilder.Build(),
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")),
                batchSize: batchSize);

            await workflow.Process(_workflowActorMockBuilder.Build());

            _fileAccessorAsserter.SourceFilesChecked();
            _workflowActorAsserter.InitialiseDuckDbCalled();

            _fileAccessorAsserter
                .TemporaryProcessingFolderWasSetUpForThisRun(sourceFiles);
                
            batchProcessingFoldersAndFiles.ForEach(folderAndFiles =>
            {
                var (batchFolder, batchOfFiles, batchNumber) = folderAndFiles;

                _fileAccessorAsserter
                    .BatchOfFilesWasPreparedForProcessing(
                        batchProcessingDirectory: batchFolder,
                        sourceFiles: batchOfFiles);

                _workflowActorAsserter
                    .ProcessSourceFileBatchCalledFor(batchFolder);

                _fileAccessorAsserter
                    .FailedSourceFileBatchMovedToFailureDirectory(
                        batchProcessingDirectory: batchFolder,
                        batchNumber: batchNumber);
            });
                
            _fileAccessorAsserter.TemporaryProcessingDirectoryWasRemovedAfterProcessing();
        }

        [Fact]
        public async Task ErrorCreatingParquetReports_ExceptionThrown()
        {
            string[] sourceFiles = ["file1", "file2"];

            var batchProcessingDirectory = Path.Combine(TemporaryProcessingDirectory, "1");

            // The workflow checks for the existence of any source files before continuing.
            _fileAccessorMockBuilder.WhereSourceFilesExist(sourceFiles);

            // The workflow calls the implementation to set up its source tables.
            _workflowActorMockBuilder.WhereDuckDbInitialisedSuccessfully();

            // The workflow sets up a temporary processing folder within the root "processing" folder
            // and moves all current files into it.  It will be files from within this temporary folder
            // that will be processed by this run of the function.
            _fileAccessorMockBuilder
                .WhereTemporaryProcessingFolderIsSetUpForThisRun(sourceFiles)
                .WhereBatchOfFilesIsPreparedForProcessing(
                    batchProcessingDirectory: batchProcessingDirectory,
                    sourceFiles: sourceFiles);

            _workflowActorMockBuilder
                .WhereSourceFileBatchIsProcessedSuccessfully(
                    batchProcessingFolder: batchProcessingDirectory);

            // The temporary processing folder that was set up for this run of the function is removed
            // after source files are read in, and because we have had at least one successful batch
            // processed, it prepares to generate a report by firstly creating the reports folder if
            // it doesn't yet exist.
            _fileAccessorMockBuilder
                .WhereTemporaryProcessingDirectoryIsRemovedAfterProcessing()
                .WhereReportsDirectoryIsCreatedForReportGeneration();

            // The workflow calls the implementation to generate its report, but the report generation
            // throws an Exception.
            _workflowActorMockBuilder.WhereReportsAreGeneratedWithErrors(
                reportsFolder: ReportsFolder,
                reportsFilenamePrefix: ReportsFilenamePrefix);

            var workflow = BuildWorkflow(
                fileAccessor: _fileAccessorMockBuilder.Build(),
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")));

            await Assert.ThrowsAsync<ArgumentException>(() => 
                workflow.Process(_workflowActorMockBuilder.Build()));

            _fileAccessorAsserter.SourceFilesChecked();
            _workflowActorAsserter.InitialiseDuckDbCalled();

            _fileAccessorAsserter
                .TemporaryProcessingFolderWasSetUpForThisRun(sourceFiles: sourceFiles)
                .BatchOfFilesWasPreparedForProcessing(
                    batchProcessingDirectory: batchProcessingDirectory,
                    sourceFiles: sourceFiles);

            _workflowActorAsserter.ProcessSourceFileBatchCalledFor(batchProcessingDirectory);

            _fileAccessorAsserter
                .TemporaryProcessingDirectoryWasRemovedAfterProcessing()
                .ReportsDirectoryWasCreatedForReportGeneration();

            _workflowActorAsserter.CreateReportWasCalled();
        }
    }

    private ProcessRequestFilesWorkflow BuildWorkflow(
        IFileAccessor fileAccessor,
        DateTimeProvider? dateTimeProvider = null,
        int? batchSize = null)
    {
        return new(
            Mock.Of<ILogger<ProcessRequestFilesWorkflow>>(),
            dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
            fileAccessor: fileAccessor,
            temporaryProcessingFolderNameGenerator: () => "temp-processing-folder",
            batchSize: batchSize ?? 100);
    }
}

public static class FileAccessorMockBuilderExtensions
{
    /// <summary>
    /// The workflow checks for the existence of any source files before continuing.
    /// </summary>
    public static FileAccessorMockBuilder WhereSourceFilesExist(
        this FileAccessorMockBuilder fileAccessorMockBuilder,
        IEnumerable<string> sourceFiles)
    {
        return fileAccessorMockBuilder
            .WhereDirectoryExists(ProcessRequestFilesWorkflowTests.SourceFolder)
            .WhereFileListForDirectoryIs(
                directory: ProcessRequestFilesWorkflowTests.SourceFolder,
                files: sourceFiles);
    }

    /// <summary>
    /// The workflow sets up a temporary processing folder within the root "processing" folder
    /// and moves all current files into it.  It will be files from within this temporary folder
    /// that will be processed by this run of the function.
    /// </summary>
    public static FileAccessorMockBuilder WhereTemporaryProcessingFolderIsSetUpForThisRun(
        this FileAccessorMockBuilder fileAccessorMockBuilder,
        IEnumerable<string> sourceFiles)
    {
        return fileAccessorMockBuilder
            .WhereDirectoryIsCreated(ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory)
            .WhereFilesAreMovedBetweenFolders(
                sourceFiles: sourceFiles,
                sourceDirectory: ProcessRequestFilesWorkflowTests.SourceFolder,
                destinationDirectory: ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory);
    }

    /// <summary>
    /// For each batch of files to process, a separate batch folder is created and the files within
    /// processed by the implementation.
    /// </summary>
    public static FileAccessorMockBuilder WhereBatchOfFilesIsPreparedForProcessing(
        this FileAccessorMockBuilder fileAccessorMockBuilder,
        string batchProcessingDirectory,
        IEnumerable<string> sourceFiles)
    {
        return fileAccessorMockBuilder
            .WhereDirectoryIsCreated(batchProcessingDirectory)
            .WhereFilesAreMovedBetweenFolders(
                sourceFiles: sourceFiles,
                sourceDirectory: ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory,
                destinationDirectory: batchProcessingDirectory);
    }
    
    /// <summary>
    /// When a batch of files fails processing, the folder containing the file batch is moved under
    /// a source-folder/failures-folder/temp-processing-folder/batch-number folder structure.
    /// </summary>
    public static FileAccessorMockBuilder WhereBatchOfFilesFailsProcessing(
        this FileAccessorMockBuilder fileAccessorMockBuilder,
        string batchProcessingDirectory,
        int batchNumber)
    {
        return fileAccessorMockBuilder
            .WhereDirectoryIsCreated(ProcessRequestFilesWorkflowTests.FailuresFolder)
            .WhereFileIsMoved(
                sourcePath: batchProcessingDirectory,
                destinationPath: Path.Combine(ProcessRequestFilesWorkflowTests.FailuresFolder, batchNumber.ToString()));
    }
    
    /// <summary>
    /// The temporary processing folder that was set up for this run of the function is removed
    /// after source files are processed.
    /// </summary>
    public static FileAccessorMockBuilder WhereTemporaryProcessingDirectoryIsRemovedAfterProcessing(
        this FileAccessorMockBuilder fileAccessorMockBuilder)
    {
        return fileAccessorMockBuilder
            .WhereDirectoryIsDeleted(ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory);
    }
    
    /// <summary>
    /// The reports folder is created, ready for new reports to be created.
    /// </summary>
    public static FileAccessorMockBuilder WhereReportsDirectoryIsCreatedForReportGeneration(
        this FileAccessorMockBuilder fileAccessorMockBuilder)
    {
        return fileAccessorMockBuilder
            .WhereDirectoryIsCreated(ProcessRequestFilesWorkflowTests.ReportsFolder);
    }
}

public static class FileAccessorMockBuilderAsserterExtensions
{
    /// <summary>
    /// The workflow checks for the existence of any source files before continuing.
    /// </summary>
    public static FileAccessorMockBuilder.Asserter SourceFilesChecked(
        this FileAccessorMockBuilder.Asserter asserter)
    {
        return asserter
            .DirectoryExistsCalledFor(ProcessRequestFilesWorkflowTests.SourceFolder)
            .FileListForDirectoryCalledFor(ProcessRequestFilesWorkflowTests.SourceFolder);
    }
    
    /// <summary>
    /// The workflow sets up a temporary processing folder within the root "processing" folder
    /// and moves all current files into it.  It will be files from within this temporary folder
    /// that will be processed by this run of the function.
    /// </summary>
    public static FileAccessorMockBuilder.Asserter TemporaryProcessingFolderWasSetUpForThisRun(
        this FileAccessorMockBuilder.Asserter asserter,
        IEnumerable<string> sourceFiles)
    {
        return asserter
            .CreateDirectoryCalledFor(ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory)
            .MoveBetweenFoldersCalledFor(
                files: sourceFiles,
                sourceDirectory: ProcessRequestFilesWorkflowTests.SourceFolder,
                destinationDirectory: ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory);
    }
    
    public static FileAccessorMockBuilder.Asserter BatchOfFilesWasPreparedForProcessing(
        this FileAccessorMockBuilder.Asserter asserter,
        string batchProcessingDirectory,
        IEnumerable<string> sourceFiles)
    {
        return asserter    
            .CreateDirectoryCalledFor(batchProcessingDirectory)
            .MoveBetweenFoldersCalledFor(
                files: sourceFiles,
                sourceDirectory: ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory,
                destinationDirectory: batchProcessingDirectory);
    }
    
    public static FileAccessorMockBuilder.Asserter FailedSourceFileBatchMovedToFailureDirectory(
        this FileAccessorMockBuilder.Asserter asserter,
        string batchProcessingDirectory,
        int batchNumber)
    {
        return asserter
            .CreateDirectoryCalledFor(ProcessRequestFilesWorkflowTests.FailuresFolder)
            .MoveFileCalledFor(
                sourcePath: batchProcessingDirectory,
                destinationPath: Path.Combine(ProcessRequestFilesWorkflowTests.FailuresFolder, batchNumber.ToString()));
    }
    
    public static FileAccessorMockBuilder.Asserter TemporaryProcessingDirectoryWasRemovedAfterProcessing(
        this FileAccessorMockBuilder.Asserter asserter)
    {
        return asserter    
            .DeleteDirectoryCalledFor(ProcessRequestFilesWorkflowTests.TemporaryProcessingDirectory);
    }

    public static FileAccessorMockBuilder.Asserter ReportsDirectoryWasCreatedForReportGeneration(
        this FileAccessorMockBuilder.Asserter asserter)
    {
        return asserter    
            .CreateDirectoryCalledFor(ProcessRequestFilesWorkflowTests.ReportsFolder);
    }
}

public static class WorkflowActorMockBuilderExtensions
{
    public static WorkflowActorMockBuilder WhereReportGeneratedSuccessfully(
        this WorkflowActorMockBuilder workflowActorMockBuilder)
    {
        return workflowActorMockBuilder
            .WhereReportsAreCreatedSuccessfully(
                reportsFolder: ProcessRequestFilesWorkflowTests.ReportsFolder,
                reportsFilenamePrefix: ProcessRequestFilesWorkflowTests.ReportsFilenamePrefix);
    }
}

public static class WorkflowActorMockBuilderAsserterExtensions
{
    public static WorkflowActorMockBuilder.Asserter CreateReportWasCalled(
        this WorkflowActorMockBuilder.Asserter asserter)
    {
        return asserter
            .CreateParquetReportsCalledFor(
                reportsFolder: ProcessRequestFilesWorkflowTests.ReportsFolder,
                reportsFilenamePrefix: "20220316-120102");
    }
}
