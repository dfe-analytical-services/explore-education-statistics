using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(_basePath, "PublicApiQueries");
    }

    public string PublicApiQueriesProcessingDirectoryPath() {
        return Path.Combine(_basePath, "PublicApiQueries", "Processing");
    }

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(_basePath, "PublicApiQueries", "Reports");
    }
}
