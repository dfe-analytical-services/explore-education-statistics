using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Functions;

public class ConsumeAnalyticsRequestFilesFunction(
    IEnumerable<IRequestFileProcessor> processors,
    ILogger<ConsumeAnalyticsRequestFilesFunction> logger)
{
    [Function(nameof(ConsumeAnalyticsRequestFilesFunction))]
    public async Task Run(
        [TimerTrigger("%App:ConsumeAnalyticsRequestFilesCronSchedule%")] TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumeAnalyticsRequestFilesFunction)} triggered");

        foreach (var requestFileProcessor in processors)
        {
            try
            {
                await requestFileProcessor.Process();
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Failed to process request files with processor {RequestFileProcessorType}",
                    requestFileProcessor.GetType());
            }
        }
    }
}
