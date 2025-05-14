using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;

public class WorkflowActorMockBuilder
{
    private readonly Mock<IWorkflowActor> _mock = new(MockBehavior.Strict);

    public WorkflowActorMockBuilder()
    {
        Assert = new Asserter(_mock);
    }
    
    public IWorkflowActor Build()
    {
        return _mock.Object;
    }

    public WorkflowActorMockBuilder WhereDuckDbInitialisedSuccessfully()
    {
        _mock
            .Setup(m => m.InitialiseDuckDb(It.IsAny<DuckDbConnection>()))
            .Returns(Task.CompletedTask);
        return this;
    }

    public WorkflowActorMockBuilder WhereSourceFilesAreProcessedSuccessfully(
        string processingFolder,
        IEnumerable<string> sourceFiles)
    {
        sourceFiles.ForEach(file =>
        {
            _mock
                .Setup(a => a.ProcessSourceFile(
                    Path.Combine(processingFolder, file),
                    It.IsAny<DuckDbConnection>()))
                .Returns(Task.CompletedTask);
        });
        return this;
    }

    public WorkflowActorMockBuilder WhereSourceFilesAreProcessedWithErrors(
        string processingFolder,
        IEnumerable<string> sourceFiles)
    {
        sourceFiles.ForEach(file =>
        {
            _mock
                .Setup(a => a.ProcessSourceFile(
                    Path.Combine(processingFolder, file),
                    It.IsAny<DuckDbConnection>()))
                .ThrowsAsync(new ArgumentException());
        });
        return this;
    }
    
    public WorkflowActorMockBuilder WhereReportsAreGeneratedSuccessfully(
        string reportsFolder,
        string reportsFilenamePrefix)
    {
        _mock
            .Setup(m => m.CreateParquetReports(
                Path.Combine(reportsFolder, reportsFilenamePrefix),
                It.IsAny<DuckDbConnection>()))
            .Returns(Task.CompletedTask);
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IWorkflowActor> mock)
    {
        public Asserter DuckDbInitialised()
        {
            mock.Verify(m => 
                m.InitialiseDuckDb(It.IsAny<DuckDbConnection>()),
                Times.Once);
            return this;
        }
        
        public Asserter SourceFilesProcessed(
            string processingFolder,
            IEnumerable<string> sourceFiles)
        {
            sourceFiles.ForEach(file =>
            {
                mock.Verify(a => 
                    a.ProcessSourceFile(
                        Path.Combine(processingFolder, file),
                        It.IsAny<DuckDbConnection>()),
                    Times.Once);
            });
            return this;
        }
        
        public Asserter ReportsGeneratedSuccessfully(
            string reportsFolder,
            string reportsFilenamePrefix)
        {
            mock.Verify(m => 
                m.CreateParquetReports(
                    Path.Combine(reportsFolder, reportsFilenamePrefix),
                    It.IsAny<DuckDbConnection>()),
                Times.Once);
            return this;
        }
    }
}
