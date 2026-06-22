using System.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
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
        _mock.Setup(s => s.GetSourceDirectory()).Returns("source");

        _mock.Setup(s => s.GetReportsDirectory()).Returns("reports");

        return _mock.Object;
    }

    public WorkflowActorMockBuilder WhereDuckDbInitialisedSuccessfully()
    {
        _mock.Setup(m => m.InitialiseDuckDb(ItIsOpenDuckDbConnection())).Returns(Task.CompletedTask);
        return this;
    }

    public WorkflowActorMockBuilder WhereDuckDbInitialisedWithErrors()
    {
        _mock
            .Setup(m => m.InitialiseDuckDb(ItIsOpenDuckDbConnection()))
            .ThrowsAsync(new ArgumentException("Mock error initialising DuckDB"));
        return this;
    }

    public WorkflowActorMockBuilder WhereSourceFileBatchIsProcessedSuccessfully(string batchProcessingFolder)
    {
        _mock
            .Setup(a => a.ProcessSourceFiles(Path.Combine(batchProcessingFolder, "*"), ItIsOpenDuckDbConnection()))
            .Returns(Task.CompletedTask);
        return this;
    }

    public WorkflowActorMockBuilder WhereSourceFilesAreProcessedWithErrors(string batchProcessingFolder)
    {
        _mock
            .Setup(a => a.ProcessSourceFiles(batchProcessingFolder, ItIsOpenDuckDbConnection()))
            .ThrowsAsync(new ArgumentException($"Mock error processing batch folder {batchProcessingFolder}"));
        return this;
    }

    public WorkflowActorMockBuilder WhereReportsAreCreatedSuccessfully(
        string reportsFolder,
        string reportsFilenamePrefix
    )
    {
        _mock
            .Setup(m =>
                m.CreateParquetReports(Path.Combine(reportsFolder, reportsFilenamePrefix), ItIsOpenDuckDbConnection())
            )
            .Returns(Task.CompletedTask);
        return this;
    }

    public WorkflowActorMockBuilder WhereReportsAreGeneratedWithErrors(
        string reportsFolder,
        string reportsFilenamePrefix
    )
    {
        _mock
            .Setup(m =>
                m.CreateParquetReports(Path.Combine(reportsFolder, reportsFilenamePrefix), ItIsOpenDuckDbConnection())
            )
            .ThrowsAsync(new ArgumentException("Mock error generating Parquet reports"));
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IWorkflowActor> mock)
    {
        public Asserter InitialiseDuckDbCalled()
        {
            mock.Verify(m => m.InitialiseDuckDb(It.IsAny<DuckDbConnection>()), Times.Once);
            return this;
        }

        public Asserter ProcessSourceFileBatchCalledFor(string batchProcessingDirectory)
        {
            mock.Verify(
                a => a.ProcessSourceFiles(Path.Combine(batchProcessingDirectory, "*"), It.IsAny<DuckDbConnection>()),
                Times.Once
            );
            return this;
        }

        public Asserter CreateParquetReportsCalledFor(string reportsFolder, string reportsFilenamePrefix)
        {
            mock.Verify(
                m =>
                    m.CreateParquetReports(
                        Path.Combine(reportsFolder, reportsFilenamePrefix),
                        It.IsAny<DuckDbConnection>()
                    ),
                Times.Once
            );
            return this;
        }
    }

    private static DuckDbConnection ItIsOpenDuckDbConnection()
    {
        return It.Is<DuckDbConnection>(c => c.State == ConnectionState.Open);
    }
}
