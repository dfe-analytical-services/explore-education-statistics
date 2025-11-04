using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessNextDataSetVersionMappingsOrchestrationTests
{
    public class ProcessNextDataSetVersionMappingsTests
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();
            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.CopyCsvFiles,
                ActivityNames.CreateMappings,
                ActivityNames.ApplyAutoMappings,
                ActivityNames.CompleteNextDataSetVersionMappingProcessing,
            ];

            foreach (var activityName in expectedActivitySequence)
            {
                mockOrchestrationContext
                    .InSequence(activitySequence)
                    .Setup(context =>
                        context.CallActivityAsync(activityName, mockOrchestrationContext.Object.InstanceId, null)
                    )
                    .Returns(Task.CompletedTask);
            }

            await ProcessNextDataSetVersionMappings(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            var activitySequence = new MockSequence();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(
                        ActivityNames.CopyCsvFiles,
                        mockOrchestrationContext.Object.InstanceId,
                        null
                    )
                )
                .Throws<Exception>();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(
                        ActivityNames.HandleProcessingFailure,
                        mockOrchestrationContext.Object.InstanceId,
                        null
                    )
                )
                .Returns(Task.CompletedTask);

            await ProcessNextDataSetVersionMappings(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessNextDataSetVersionMappings(TaskOrchestrationContext orchestrationContext)
        {
            await ProcessNextDataSetVersionMappingsFunctionOrchestration.ProcessNextDataSetVersionMappings(
                orchestrationContext,
                new ProcessDataSetVersionContext { DataSetVersionId = Guid.NewGuid() }
            );
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(Guid? instanceId = null)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock.Setup(context =>
                    context.CreateReplaySafeLogger(
                        nameof(ProcessNextDataSetVersionMappingsFunctionOrchestration.ProcessNextDataSetVersionMappings)
                    )
                )
                .Returns(NullLogger.Instance);

            mock.SetupGet(context => context.InstanceId).Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            return mock;
        }
    }
}
