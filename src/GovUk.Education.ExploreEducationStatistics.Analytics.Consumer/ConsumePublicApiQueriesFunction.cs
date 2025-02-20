using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public class ConsumePublicApiQueriesFunction(
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicApiQueriesFunction> logger)
{
    [Function(nameof(ConsumePublicApiQueriesFunction))]
    public async Task Run(
        [TimerTrigger("%App:ConsumePublicApiQueriesCronSchedule%")]
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumePublicApiQueriesFunction)} triggered");

        var directory = pathResolver.PublicApiQueriesDirectoryPath();
    }
}
