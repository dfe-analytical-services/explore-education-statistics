using System.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;

public class WorkflowActorMockBuilder<TRequestFileProcessor>
    where TRequestFileProcessor : IRequestFileProcessor
{
    private readonly Mock<IWorkflowActor<TRequestFileProcessor>> _mock = new(MockBehavior.Strict);

    public WorkflowActorMockBuilder()
    {
        Assert = new Asserter(_mock);
    }
    
    public IWorkflowActor<TRequestFileProcessor> Build()
    {
        return _mock.Object;
    }

    public WorkflowActorMockBuilder<TRequestFileProcessor> WhereDuckDbInitialisedSuccessfully()
    {
        _mock
            .Setup(m => m.InitialiseDuckDb(ItIsOpenDuckDbConnection()))
            .Returns(Task.CompletedTask);
        return this;
    }
    
    public WorkflowActorMockBuilder<TRequestFileProcessor> WhereDuckDbInitialisedWithErrors()
    {
        _mock
            .Setup(m => m.InitialiseDuckDb(ItIsOpenDuckDbConnection()))
            .ThrowsAsync(new ArgumentException("Mock error initialising DuckDB"));
        return this;
    }

    public WorkflowActorMockBuilder<TRequestFileProcessor> WhereSourceFilesAreProcessedSuccessfully(
        string processingFolder,
        IEnumerable<string> sourceFiles)
    {
        sourceFiles.ForEach(file =>
        {
            _mock
                .Setup(a => a.ProcessSourceFile(
                    Path.Combine(processingFolder, file),
                    ItIsOpenDuckDbConnection()))
                .Returns(Task.CompletedTask);
        });
        return this;
    }

    public WorkflowActorMockBuilder<TRequestFileProcessor> WhereSourceFilesAreProcessedWithErrors(
        string processingFolder,
        IEnumerable<string> sourceFiles)
    {
        sourceFiles.ForEach(file =>
        {
            _mock
                .Setup(a => a.ProcessSourceFile(
                    Path.Combine(processingFolder, file),
                    ItIsOpenDuckDbConnection()))
                .ThrowsAsync(new ArgumentException($"Mock error processing file {file}"));
        });
        return this;
    }
    
    public WorkflowActorMockBuilder<TRequestFileProcessor> WhereReportsAreGeneratedSuccessfully(
        string reportsFolder,
        string reportsFilenamePrefix)
    {
        _mock
            .Setup(m => m.CreateParquetReports(
                Path.Combine(reportsFolder, reportsFilenamePrefix),
                ItIsOpenDuckDbConnection()))
            .Returns(Task.CompletedTask);
        return this;
    }
    
    public WorkflowActorMockBuilder<TRequestFileProcessor> WhereReportsAreGeneratedWithErrors(
        string reportsFolder,
        string reportsFilenamePrefix)
    {
        _mock
            .Setup(m => m.CreateParquetReports(
                Path.Combine(reportsFolder, reportsFilenamePrefix),
                ItIsOpenDuckDbConnection()))
            .ThrowsAsync(new ArgumentException("Mock error generating Parquet reports"));
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IWorkflowActor<TRequestFileProcessor>> mock)
    {
        public Asserter InitialiseDuckDbCalled()
        {
            mock.Verify(m => 
                m.InitialiseDuckDb(It.IsAny<DuckDbConnection>()),
                Times.Once);
            return this;
        }
        
        public Asserter ProcessSourceFileCalledFor(
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
        
        public Asserter CreateParquetReportsCalledFor(
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

    private static DuckDbConnection ItIsOpenDuckDbConnection()
    {
        return It.Is<DuckDbConnection>(c => c.State == ConnectionState.Open);   
    }
}
