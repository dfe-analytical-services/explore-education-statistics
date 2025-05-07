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

    // PublicApiQueries
    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "queries");
    }

    public string PublicApiQueriesProcessingDirectoryPath() {
        return Path.Combine(_basePath, "public-api", "queries", "processing");
    }

    public string PublicApiQueriesFailuresDirectoryPath() {
        return Path.Combine(_basePath, "public-api", "queries", "failures");
    }

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "queries");
    }

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsProcessingDirectoryPath() {
        return Path.Combine(_basePath, "public", "zip-downloads", "processing");
    }

    public string PublicZipDownloadsFailuresDirectoryPath() {
        return Path.Combine(_basePath, "public", "zip-downloads", "failures");
    }

    public string PublicZipDownloadsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public", "zip-downloads");
    }
}
