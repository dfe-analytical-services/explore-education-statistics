using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsManager : IAnalyticsManager
{

    private readonly ILogger<IAnalyticsManager> _logger;

    private IOptions<AnalyticsOptions> _analyticsOptions;

    public AnalyticsManager(ILogger<IAnalyticsManager> logger)
    {
        _logger = logger;
    }

    public void AddZipDownload(string test)
    {
        _logger.LogWarning($"IT'S ON!!!!!! AnalyticsManager logging {test}");
    }
}
