using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        [Fact]
        public async Task NoSourceFolder_NoReportsProduced()
        {
            var actor = new Mock<IWorkflowActor>(MockBehavior.Strict);
            var fileAccessor = new Mock<IFileAccessor>(MockBehavior.Strict);

            fileAccessor
                .Setup(f => f.DirectoryExists(SourceFolder))
                .Returns(false);
            
            var workflow = BuildWorkflow(
                actor: actor.Object,
                fileAccessor: fileAccessor.Object);

            await workflow.Process();
            
            // Ensure no reports were generated or reports folder created
            // because no source folder existed.
            Mock.VerifyAll(fileAccessor);
        }
        
        [Fact]
        public async Task NoSourceFilesToConsume_NoReportsProduced()
        {
            var actor = new Mock<IWorkflowActor>(MockBehavior.Strict);
            var fileAccessor = new Mock<IFileAccessor>(MockBehavior.Strict);

            fileAccessor
                .Setup(f => f.DirectoryExists(SourceFolder))
                .Returns(true);
            
            fileAccessor
                .Setup(f => f.ListFiles(SourceFolder))
                .Returns([]);
            
            var workflow = BuildWorkflow(
                actor: actor.Object,
                fileAccessor: fileAccessor.Object);
            
            await workflow.Process();
            
            // Ensure no reports were generated or reports folder created
            // because no source files existed.
            Mock.VerifyAll(fileAccessor);
        }
        
        [Fact]
        public async Task SourceFilesExist_ReportsProduced()
        {
            var actor = new Mock<IWorkflowActor>(MockBehavior.Strict);
            var fileAccessor = new Mock<IFileAccessor>(MockBehavior.Strict);

            fileAccessor
                .Setup(f => f.DirectoryExists(SourceFolder))
                .Returns(true);

            IList<string> sourceFiles = ["file1", "file2"];
            
            fileAccessor
                .Setup(f => f.ListFiles(SourceFolder))
                .Returns(sourceFiles);

            fileAccessor
                .Setup(f => f.CreateDirectory(ProcessingFolder));

            actor
                .Setup(a => a.InitialiseDuckDb(It.IsAny<DuckDbConnection>()))
                .Returns(Task.CompletedTask);
            
            sourceFiles.ForEach(file =>
            {
                fileAccessor
                    .Setup(f => f.Move(
                        Path.Combine(SourceFolder, file),
                        Path.Combine(ProcessingFolder, file)));
            });
            
            fileAccessor
                .Setup(f => f.ListFiles(ProcessingFolder))
                .Returns(sourceFiles);

            sourceFiles.ForEach(file =>
            {
                actor
                    .Setup(a => a.ProcessSourceFile(
                        Path.Combine(ProcessingFolder, file),
                        It.IsAny<DuckDbConnection>()))
                    .Returns(Task.CompletedTask);
            });
            
            fileAccessor
                .Setup(f => f.CreateDirectory(ReportsFolder));
            
            actor
                .Setup(a => a.CreateParquetReports(
                    Path.Combine(ReportsFolder, "20220316-120102"),
                    It.IsAny<DuckDbConnection>()))
                .Returns(Task.CompletedTask);

            fileAccessor
                .Setup(f => f.DeleteDirectory(ProcessingFolder));
            
            var workflow = BuildWorkflow(
                actor: actor.Object,
                fileAccessor: fileAccessor.Object,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")));

            await workflow.Process();
            
            // Ensure the workflow created the reports directory for the
            // generated report to be placed into.
            Mock.VerifyAll(actor, fileAccessor);
        }
        
        [Fact]
        public async Task ErrorsProcessingFiles_MovedToFailuresFolderButReportGenerated()
        {
            var actor = new Mock<IWorkflowActor>(MockBehavior.Strict);
            var fileAccessor = new Mock<IFileAccessor>(MockBehavior.Strict);

            fileAccessor
                .Setup(f => f.DirectoryExists(SourceFolder))
                .Returns(true);

            IList<string> sourceFiles = [
                "succeedingFile1",
                "failingFile2",
                "succeedingFile3",
                "failingFile4"
            ];
            
            fileAccessor
                .Setup(f => f.ListFiles(SourceFolder))
                .Returns(sourceFiles);

            fileAccessor
                .Setup(f => f.CreateDirectory(ProcessingFolder));

            actor
                .Setup(a => a.InitialiseDuckDb(It.IsAny<DuckDbConnection>()))
                .Returns(Task.CompletedTask);
            
            sourceFiles.ForEach(file =>
            {
                fileAccessor
                    .Setup(f => f.Move(
                        Path.Combine(SourceFolder, file),
                        Path.Combine(ProcessingFolder, file)));
            });
            
            fileAccessor
                .Setup(f => f.ListFiles(ProcessingFolder))
                .Returns(sourceFiles);

            new List<string> {"succeedingFile1", "succeedingFile3"}.ForEach(file =>
            {
                actor
                    .Setup(a => a.ProcessSourceFile(
                        Path.Combine(ProcessingFolder, file),
                        It.IsAny<DuckDbConnection>()))
                    .Returns(Task.CompletedTask);
            });
            
            fileAccessor
                .Setup(f => f.CreateDirectory(FailuresFolder));
            
            new List<string> {"failingFile2", "failingFile4"}.ForEach(file =>
            {
                actor
                    .Setup(a => a.ProcessSourceFile(
                        Path.Combine(ProcessingFolder, file),
                        It.IsAny<DuckDbConnection>()))
                    .ThrowsAsync(new ArgumentException());
                
                fileAccessor
                    .Setup(f => f.Move(
                        Path.Combine(SourceFolder, file),
                        Path.Combine(FailuresFolder, file)));
            });
            
            fileAccessor
                .Setup(f => f.CreateDirectory(ReportsFolder));
            
            actor
                .Setup(a => a.CreateParquetReports(
                    Path.Combine(ReportsFolder, "20220316-120102"),
                    It.IsAny<DuckDbConnection>()))
                .Returns(Task.CompletedTask);

            fileAccessor
                .Setup(f => f.DeleteDirectory(ProcessingFolder));
            
            var workflow = BuildWorkflow(
                actor: actor.Object,
                fileAccessor: fileAccessor.Object,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2022-03-16T12:01:02Z")));

            await workflow.Process();
            
            // Ensure the workflow created the reports directory for the
            // generated report to be placed into.
            Mock.VerifyAll(actor, fileAccessor);
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
