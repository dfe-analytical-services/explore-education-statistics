using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Entities;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;

public static class TaskOrchestrationContextExtensions
{
    public static async Task CallActivity(
        this TaskOrchestrationContext context,
        TaskName name,
        ILogger logger,
        object? input = null,
        TaskOptions? options = null
    )
    {
        logger.LogInformation(
            "Calling activity '{ActivityName}' (InstanceId={InstanceId})",
            name,
            context.InstanceId
        );

        await context.CallActivityAsync(name, input, options);

        logger.LogInformation(
            "Activity '{ActivityName}' completed (InstanceId={InstanceId})",
            name,
            context.InstanceId
        );
    }

    public static async Task CallActivityExclusively(
        this TaskOrchestrationContext context,
        TaskName name,
        ILogger logger,
        object? input = null,
        TaskOptions? options = null
    )
    {
        // Create an entity id which represents the activity name
        var activityLockId = new EntityInstanceId(nameof(ActivityLock), name);

        logger.LogInformation(
            "Attempting to acquire lock '{EntityId}' (InstanceId={InstanceId})",
            activityLockId,
            context.InstanceId
        );

        // Acquire a lock on the entity instance ensuring that no other orchestrations can execute the same activity
        // concurrently provided that they also acquire a lock on the same entity.
        // The lock is automatically released at the end of the using statement.
        await using (await context.Entities.LockEntitiesAsync(activityLockId))
        {
            logger.LogInformation(
                "Acquired lock '{EntityId}' (InstanceId={InstanceId})",
                activityLockId,
                context.InstanceId
            );

            logger.LogInformation(
                "Calling activity '{ActivityName}' (InstanceId={InstanceId})",
                name,
                context.InstanceId
            );

            await context.CallActivityAsync(name, input, options);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId})",
                name,
                context.InstanceId
            );

            logger.LogInformation(
                "Releasing lock '{EntityId}' (InstanceId={InstanceId})",
                activityLockId,
                context.InstanceId
            );
        }
    }
}
