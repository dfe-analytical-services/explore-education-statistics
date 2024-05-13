using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessInitialDataSetVersionFunction
{
    [Function(nameof(ProcessInitialDataSetVersion))]
    public async Task ProcessInitialDataSetVersion(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessInitialDataSetVersionContext input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessInitialDataSetVersion));

        logger.LogInformation(
            "Processing initial data set version (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId);

        try
        {
            // Other activity function calls to be added here to cover the following stages:
            // Move CSV files from Azure Blob Storage to Azure File Share
            // Create meta summary for the DataSetVersion
            // Import metadata to DuckDb
            // Import data to DuckDb
            // Export to Parquet files

            await context.CallActivityAsync(nameof(CompleteProcessingFunction.CompleteProcessing), input);

            logger.LogInformation(
                "Activity '{ActivityName}' completed (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                nameof(CompleteProcessingFunction.CompleteProcessing),
                context.InstanceId,
                input.DataSetVersionId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                input.DataSetVersionId);

            await context.CallActivityAsync(nameof(HandleProcessingFailureFunction.HandleProcessingFailure), input);
        }
    }
}
