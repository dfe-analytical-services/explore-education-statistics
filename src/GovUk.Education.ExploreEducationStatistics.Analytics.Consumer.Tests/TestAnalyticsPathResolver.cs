using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver, IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }

    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    public string BasePath()
    {
        return _basePath;
    }

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api");
    }

    public string PublicApiQueriesProcessingDirectoryPath() {
        return Path.Combine(_basePath, "public-api", "processing");
    }

    public string PublicApiQueriesFailuresDirectoryPath() {
        return Path.Combine(_basePath, "public-api", "failures");
    }

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "reports");
    }
}
