using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api");
    }

    public string PublicApiQueriesProcessingDirectoryPath() {
        return Path.Combine(_basePath, "public-api", "processing");
    }

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "reports");
    }
}
