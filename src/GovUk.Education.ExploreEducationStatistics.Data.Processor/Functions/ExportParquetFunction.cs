#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ProcessorQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions;

public class ExportParquetFunction(ILogger<ExportParquetFunction> logger, IParquetDataSetService parquetDataSetService)
{
    [Function("ExportParquet")]
    public async Task ExportParquet(
        [QueueTrigger(ParquetExportPendingQueue)] ExportParquetMessage message,
        FunctionContext context,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "{FunctionName} exporting data file {FileId} to parquet",
            context.FunctionDefinition.Name,
            message.FileId
        );

        await parquetDataSetService.ExportStatsDbDataSet(message.FileId, cancellationToken);

        logger.LogInformation(
            "{FunctionName} completed for data file {FileId}",
            context.FunctionDefinition.Name,
            message.FileId
        );
    }
}
