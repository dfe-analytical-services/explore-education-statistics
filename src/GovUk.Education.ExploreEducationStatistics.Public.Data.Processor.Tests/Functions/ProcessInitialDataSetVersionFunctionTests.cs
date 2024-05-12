using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public class ProcessInitialDataSetVersionFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class ProcessInitialDataSetVersionTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var processingContext = new ProcessInitialDataSetVersionContext
            {
                DataSetVersionId = Guid.NewGuid()
            };

            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            mockOrchestrationContext
                .Setup(context =>
                    context.CallActivityAsync(
                        nameof(ProcessInitialDataSetVersionFunction.CompleteProcessing),
                        processingContext,
                        null))
                .Returns(Task.CompletedTask);

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.ProcessInitialDataSetVersion(
                mockOrchestrationContext.Object,
                processingContext);

            VerifyAllMocks(mockOrchestrationContext);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var processingContext = new ProcessInitialDataSetVersionContext
            {
                DataSetVersionId = Guid.NewGuid()
            };

            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            mockOrchestrationContext
                .Setup(context =>
                    context.CallActivityAsync(
                        nameof(ProcessInitialDataSetVersionFunction.CompleteProcessing),
                        processingContext,
                        null))
                .Throws<Exception>();

            mockOrchestrationContext
                .Setup(context =>
                    context.CallActivityAsync(
                        nameof(ProcessInitialDataSetVersionFunction.HandleProcessingFailure),
                        processingContext,
                        null))
                .Returns(Task.CompletedTask);

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.ProcessInitialDataSetVersion(
                mockOrchestrationContext.Object,
                processingContext);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(
            Guid? instanceId = null,
            bool isReplaying = false)
        {
            var mock = new Mock<TaskOrchestrationContext>();

            mock
                .Protected()
                .Setup<ILoggerFactory>("LoggerFactory")
                .Returns(NullLoggerFactory.Instance);

            mock
                .SetupGet(context => context.InstanceId)
                .Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            mock
                .SetupGet(context => context.IsReplaying)
                .Returns(isReplaying);

            return mock;
        }
    }

    public class HandleProcessingFailureTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var instanceId = Guid.NewGuid();

            DataSet dataSet = DataFixture.DefaultDataSet();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithStatus(DataSetVersionStatus.Processing)
                .WithImports(
                    () => DataFixture
                        .DefaultDataSetVersionImport()
                        .WithInstanceId(instanceId)
                        .WithStage(DataSetVersionImportStage.Pending)
                        .Generate(1)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(
                context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                }
            );

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.HandleProcessingFailure(
                new ProcessInitialDataSetVersionContext
                {
                    DataSetVersionId = dataSetVersion.Id
                },
                instanceId,
                CancellationToken.None
            );

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedDataSetVersion = await publicDataDbContext.DataSetVersions
                .Include(dsv => dsv.Imports)
                .FirstAsync(dsv => dsv.Id == dataSetVersion.Id);

            Assert.Equal(DataSetVersionStatus.Failed, savedDataSetVersion.Status);

            var savedImport = savedDataSetVersion.Imports.Single();
            Assert.Equal(DataSetVersionImportStage.Pending, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();
        }
    }

    public class CompleteProcessingTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessorFunctionsIntegrationTest(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var instanceId = Guid.NewGuid();

            DataSet dataSet = DataFixture
                .DefaultDataSet();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithStatus(DataSetVersionStatus.Processing)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .WithInstanceId(instanceId)
                    .WithStage(DataSetVersionImportStage.Pending)
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.CompleteProcessing(new ProcessInitialDataSetVersionContext
                {
                    DataSetVersionId = dataSetVersion.Id
                },
                instanceId,
                CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedDataSetVersion = await publicDataDbContext.DataSetVersions
                .Include(dsv => dsv.Imports)
                .FirstAsync(dsv => dsv.Id == dataSetVersion.Id);

            Assert.Equal(DataSetVersionStatus.Draft, savedDataSetVersion.Status);

            var savedImport = savedDataSetVersion.Imports.Single();
            Assert.Equal(DataSetVersionImportStage.Completing, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();
        }
    }
}
