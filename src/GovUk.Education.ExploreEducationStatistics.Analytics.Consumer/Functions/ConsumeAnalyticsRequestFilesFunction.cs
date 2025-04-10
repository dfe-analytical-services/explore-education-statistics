using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Functions;

public class ConsumeAnalyticsRequestFilesFunction(
    IEnumerable<IRequestFileProcessorService> requestFileProcessorServices,
    ILogger<ConsumeAnalyticsRequestFilesFunction> logger)
{
    [Function(nameof(ConsumeAnalyticsRequestFilesFunction))]
    public async Task Run(
        [TimerTrigger("%App:ConsumeAnalyticsRequestFilesCronSchedule%")] // @MarkFix create variable
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumeAnalyticsRequestFilesFunction)} triggered");

        foreach (var requestFileProcessorService in requestFileProcessorServices)
        {
            await requestFileProcessorService.Consume();
        }
    }
}
