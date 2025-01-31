using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Entities;
using Microsoft.DurableTask.Entities;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;

internal static class MockTaskOrchestrationEntityFeatureExtensions
{
    internal static IReturnsResult<TaskOrchestrationEntityFeature> SetupLockForActivity(
        this Mock<TaskOrchestrationEntityFeature> mock,
        string name)
    {
        return mock.Setup(entityFeature => entityFeature.LockEntitiesAsync(
                new EntityInstanceId(nameof(ActivityLock), name)))
            .ReturnsAsync(new NoopAsyncDisposable());
    }

    private sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
    }
}
