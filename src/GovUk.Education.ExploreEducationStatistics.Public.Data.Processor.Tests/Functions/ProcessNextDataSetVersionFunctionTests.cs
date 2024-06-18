using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessNextDataSetVersionFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class ProcessNextDataSetVersionTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            // Expect an entity lock to be acquired for calling the ImportMetadata activity
            var mockEntityFeature = new Mock<TaskOrchestrationEntityFeature>(MockBehavior.Strict);
            mockEntityFeature.SetupLockForActivity(ActivityNames.ImportMetadata);
            mockOrchestrationContext.SetupGet(context => context.Entities)
                .Returns(mockEntityFeature.Object);

            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.CopyCsvFiles,
                ActivityNames.CreateMappings,
                ActivityNames.CompleteProcessing,
            ];

            foreach (var activityName in expectedActivitySequence)
            {
                mockOrchestrationContext
                    .InSequence(activitySequence)
                    .Setup(context => context.CallActivityAsync(activityName,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                    .Returns(Task.CompletedTask);
            }

            await ProcessNextDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext, mockEntityFeature);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            var activitySequence = new MockSequence();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(ActivityNames.CopyCsvFiles,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Throws<Exception>();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(ActivityNames.HandleProcessingFailure,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Returns(Task.CompletedTask);

            await ProcessNextDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessNextDataSetVersion(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.ProcessNextDataSetVersion(
                orchestrationContext,
                new ProcessDataSetVersionContext {DataSetVersionId = Guid.NewGuid()});
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(
            Guid? instanceId = null,
            bool isReplaying = false)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

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
