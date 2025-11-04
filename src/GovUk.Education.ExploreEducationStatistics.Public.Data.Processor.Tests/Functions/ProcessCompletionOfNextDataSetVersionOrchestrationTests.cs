using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessCompletionOfNextDataSetVersionOrchestrationTests
{
    public class ProcessCompletionOfNextDataSetVersionImportTests
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();
            var activitySequence = new MockSequence();

            // Expect an entity lock to be acquired for calling the ImportMetadata activity
            var mockEntityFeature = new Mock<TaskOrchestrationEntityFeature>(MockBehavior.Strict);
            mockEntityFeature.SetupLockForActivity(ActivityNames.ImportMetadata);
            mockOrchestrationContext.SetupGet(context => context.Entities).Returns(mockEntityFeature.Object);

            string[] expectedActivitySequence =
            [
                ActivityNames.ImportMetadata,
                ActivityNames.CreateChanges,
                ActivityNames.ImportData,
                ActivityNames.WriteDataFiles,
                ActivityNames.CompleteNextDataSetVersionImportProcessing,
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

            await ProcessCompletionOfNextDataSetVersionImport(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext, mockEntityFeature);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            var activitySequence = new MockSequence();

            var mockEntityFeature = new Mock<TaskOrchestrationEntityFeature>(MockBehavior.Strict);
            mockEntityFeature.SetupLockForActivity(ActivityNames.ImportMetadata);
            mockOrchestrationContext.SetupGet(context => context.Entities).Returns(mockEntityFeature.Object);

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(
                        ActivityNames.ImportMetadata,
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

            await ProcessCompletionOfNextDataSetVersionImport(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private static async Task ProcessCompletionOfNextDataSetVersionImport(
            TaskOrchestrationContext orchestrationContext
        )
        {
            await ProcessCompletionOfNextDataSetVersionOrchestration.ProcessCompletionOfNextDataSetVersionImport(
                orchestrationContext,
                new ProcessDataSetVersionContext { DataSetVersionId = Guid.NewGuid() }
            );
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(Guid? instanceId = null)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock.Setup(context =>
                    context.CreateReplaySafeLogger(
                        nameof(
                            ProcessCompletionOfNextDataSetVersionOrchestration.ProcessCompletionOfNextDataSetVersionImport
                        )
                    )
                )
                .Returns(NullLogger.Instance);

            mock.SetupGet(context => context.InstanceId).Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            return mock;
        }
    }
}
