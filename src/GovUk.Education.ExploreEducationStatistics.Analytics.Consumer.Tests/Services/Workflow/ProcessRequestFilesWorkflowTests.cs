using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow;

public abstract class ProcessRequestFilesWorkflowTests
{
    public class ProcessTests : ProcessRequestFilesWorkflowTests
    {
        private const string SourceFolder = "source";
        private static readonly string ProcessingFolder = Path.Combine(SourceFolder, "processing");
        private const string ReportsFolder = "reports";
        private static readonly string FailuresFolder = Path.Combine(SourceFolder, "failures");
        
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
            var actor = new Mock<IWorkflowActor>(MockBehavior.Strict);

            var fileAccessor = _fileAccessorMockBuilder
                .WhereDirectoryDoesNotExist(SourceFolder)
                .Build();

            var workflow = BuildWorkflow(
                actor: actor.Object,
                fileAccessor: fileAccessor);

            await workflow.Process();
            
            // Ensure no reports were generated or reports folder created
            // because no source folder existed.
            _fileAccessorAsserter.DirectoryExistsCalledFor(SourceFolder);
        }
        
        [Fact]
        public async Task NoSourceFilesToConsume_NoReportsProduced()
        {
            var actor = new Mock<IWorkflowActor>(MockBehavior.Strict);

            var fileAccessor = _fileAccessorMockBuilder
                .WhereDirectoryExists(SourceFolder)
                .WhereFileListForDirectoryIs(SourceFolder, [])
                .Build();
            
            var workflow = BuildWorkflow(
                actor: actor.Object,
                fileAccessor: fileAccessor);
            
            await workflow.Process();
            
            // Ensure no reports were generated or reports folder created
            // because no source files existed.
            _fileAccessorAsserter.DirectoryExistsCalledFor(SourceFolder);
            _fileAccessorAsserter.FileListForDirectoryCalledFor(SourceFolder);
        }
        
        [Fact]
        public async Task SourceFilesExist_ReportsProduced()
        {
            string[] sourceFiles = ["file1", "file2"];

            _fileAccessorMockBuilder
                .WhereDirectoryExists(SourceFolder)
                .WhereFileListForDirectoryIs(
                    directory: SourceFolder,
                    files: sourceFiles)
                .WhereDirectoryIsCreated(ProcessingFolder);

            _workflowActorMockBuilder
                .WhereDuckDbInitialisedSuccessfully();

            _fileAccessorMockBuilder
                .WhereFilesAreMovedBetweenFolders(
                    sourceFiles: sourceFiles,
                    sourceDirectory: SourceFolder,
                    destinationDirectory: ProcessingFolder)
                .WhereFileListForDirectoryIs(ProcessingFolder, sourceFiles);

            _workflowActorMockBuilder
                .WhereSourceFilesAreProcessedSuccessfully(
                    processingFolder: ProcessingFolder,
                    sourceFiles: sourceFiles);

            _fileAccessorMockBuilder
                .WhereDirectoryIsDeleted(ProcessingFolder)
                .WhereDirectoryIsCreated(ReportsFolder);

            _workflowActorMockBuilder
                .WhereReportsAreGeneratedSuccessfully(
                    reportsFolder: ReportsFolder,
                    reportsFilenamePrefix: "20220316-120102");
            
            var workflow = BuildWorkflow(
                actor: _workflowActorMockBuilder.Build(),
                fileAccessor: _fileAccessorMockBuilder.Build(),
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")));

            await workflow.Process();
            
            _fileAccessorAsserter.DirectoryExistsCalledFor(SourceFolder);
            _fileAccessorAsserter.FileListForDirectoryCalledFor(SourceFolder);
            _fileAccessorAsserter.CreateDirectoryCalledFor(ProcessingFolder);
            
            _fileAccessorAsserter.MoveBetweenFoldersCalledFor(
                sourceFiles, SourceFolder, ProcessingFolder);
            
            _workflowActorAsserter.DuckDbInitialised();
            _workflowActorAsserter.SourceFilesProcessed(
                processingFolder: ProcessingFolder,
                sourceFiles: sourceFiles);
            _fileAccessorAsserter.DeleteDirectoryCalledFor(ProcessingFolder);
            
            _fileAccessorAsserter.CreateDirectoryCalledFor(ReportsFolder);
            _workflowActorAsserter.ReportsGeneratedSuccessfully(
                reportsFolder: ReportsFolder,
                reportsFilenamePrefix: "20220316-120102");
        }
        
        [Fact]
        public async Task ErrorsProcessingFiles_MovedToFailuresFolderButReportGenerated()
        {
            IList<string> sourceFiles = [
                "succeedingFile1",
                "failingFile2",
                "succeedingFile3",
                "failingFile4"
            ];
            
            _fileAccessorMockBuilder
                .WhereDirectoryExists(SourceFolder)
                .WhereFileListForDirectoryIs(
                    directory: SourceFolder,
                    files: sourceFiles)
                .WhereDirectoryIsCreated(ProcessingFolder);

            _workflowActorMockBuilder
                .WhereDuckDbInitialisedSuccessfully();

            _fileAccessorMockBuilder
                .WhereFilesAreMovedBetweenFolders(
                    sourceFiles: sourceFiles,
                    sourceDirectory: SourceFolder,
                    destinationDirectory: ProcessingFolder)
                .WhereFileListForDirectoryIs(ProcessingFolder, sourceFiles);

            _workflowActorMockBuilder
                .WhereSourceFilesAreProcessedSuccessfully(
                    processingFolder: ProcessingFolder,
                    sourceFiles: ["succeedingFile1", "succeedingFile3"])
                .WhereSourceFilesAreProcessedWithErrors(
                    processingFolder: ProcessingFolder,
                    sourceFiles: ["failingFile2", "failingFile4"]);

            _fileAccessorMockBuilder
                .WhereDirectoryIsCreated(FailuresFolder)
                .WhereFilesAreMovedBetweenFolders(
                    sourceFiles: ["failingFile2", "failingFile4"],
                    sourceDirectory: ProcessingFolder,
                    destinationDirectory: FailuresFolder);

            _fileAccessorMockBuilder
                .WhereDirectoryIsDeleted(ProcessingFolder)
                .WhereDirectoryIsCreated(ReportsFolder);

            _workflowActorMockBuilder
                .WhereReportsAreGeneratedSuccessfully(
                    reportsFolder: ReportsFolder,
                    reportsFilenamePrefix: "20220316-120102");
            
            var workflow = BuildWorkflow(
                actor: _workflowActorMockBuilder.Build(),
                fileAccessor: _fileAccessorMockBuilder.Build(),
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")));

            await workflow.Process();
            
            _fileAccessorAsserter.DirectoryExistsCalledFor(SourceFolder);
            _fileAccessorAsserter.FileListForDirectoryCalledFor(SourceFolder);
            _fileAccessorAsserter.CreateDirectoryCalledFor(ProcessingFolder);
            
            _fileAccessorAsserter.MoveBetweenFoldersCalledFor(
                sourceFiles, SourceFolder, ProcessingFolder);
            
            _workflowActorAsserter.DuckDbInitialised();
            _workflowActorAsserter.SourceFilesProcessed(
                processingFolder: ProcessingFolder,
                sourceFiles: sourceFiles);
            
            _fileAccessorAsserter.CreateDirectoryCalledFor(FailuresFolder);
            _fileAccessorAsserter.MoveBetweenFoldersCalledFor(
                ["failingFile2", "failingFile4"], ProcessingFolder, FailuresFolder);
            
            _fileAccessorAsserter.DeleteDirectoryCalledFor(ProcessingFolder);
            
            _fileAccessorAsserter.CreateDirectoryCalledFor(ReportsFolder);
            _workflowActorAsserter.ReportsGeneratedSuccessfully(
                reportsFolder: ReportsFolder,
                reportsFilenamePrefix: "20220316-120102");
        }
    }

    private ProcessRequestFilesWorkflow BuildWorkflow(
        IWorkflowActor actor,
        IFileAccessor fileAccessor,
        DateTimeProvider? dateTimeProvider = null)
    {
        return new(
            processorName: nameof(ProcessRequestFilesWorkflowTests),
            sourceDirectory: "source",
            reportsDirectory: "reports",
            actor: actor,
            Mock.Of<ILogger<IRequestFileProcessor>>(),
            dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
            fileAccessor: fileAccessor);
    }
}
