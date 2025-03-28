using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver
{
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
        return Path.Combine(_basePath, "public-api", "queries");
    }

    public string PublicApiQueriesProcessingDirectoryPath() {
        return Path.Combine(_basePath, "public-api", "queries", "processing");
    }

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "queries");
    }

    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsProcessingDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads", "processing");

    }

    public string PublicZipDownloadsReportsDirectoryPath()
    {
        return Path.Combine(_basePath, "reports", "public", "zip-downloads");
    }
}
