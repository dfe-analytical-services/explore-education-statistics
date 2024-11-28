using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class LongRunningFunctions(ILogger<LongRunningFunctions> logger)
{
    [Function(nameof(LongRunningActivity))]
    public async Task LongRunningActivity(
        [ActivityTrigger] LongRunningOrchestrationContext input,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed.Seconds < input.DurationSeconds)
        {
            await Task.Delay(10000, cancellationToken);

            logger.LogInformation($"Long-running orchestration running for {stopwatch.Elapsed.Seconds} " +
                                  $"out of {input.DurationSeconds} seconds");
        }
    }
}
