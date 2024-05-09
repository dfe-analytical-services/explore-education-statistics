using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.DurableTask;
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
                        nameof(CompleteProcessingFunction.CompleteProcessing),
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
                        nameof(CompleteProcessingFunction.CompleteProcessing),
                        processingContext,
                        null))
                .Throws<Exception>();

            mockOrchestrationContext
                .Setup(context =>
                    context.CallActivityAsync(
                        nameof(HandleProcessingFailureFunction.HandleProcessingFailure),
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
}
