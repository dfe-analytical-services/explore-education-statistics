using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessNextDataSetVersionFunction(
    PublicDataDbContext publicDataDbContext) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(nameof(ProcessNextDataSetVersion))]
    public async Task ProcessNextDataSetVersion(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessDataSetVersionContext input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessNextDataSetVersion));

        logger.LogInformation(
            "Processing next data set version (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId);

        try
        {
            await context.CallActivity(ActivityNames.CopyCsvFiles, logger, context.InstanceId);
            await context.CallActivityExclusively(ActivityNames.ImportMetadata, logger,
                context.InstanceId);
            await context.CallActivity(ActivityNames.CompleteProcessing, logger, context.InstanceId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivity(ActivityNames.HandleProcessingFailure, logger, context.InstanceId);
        }
    }
}
