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
    
    // PublicApiQueries
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "get-meta");
    }

    public string PublicApiDataSetVersionCallsProcessingDirectoryPath() {
        return Path.Combine(PublicApiDataSetVersionCallsDirectoryPath(), "processing");
    }

    public string PublicApiDataSetVersionCallsFailuresDirectoryPath() {
        return Path.Combine(PublicApiDataSetVersionCallsDirectoryPath(), "failures");
    }

    public string PublicApiDataSetVersionCallsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "get-meta");
    }

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsProcessingDirectoryPath() {
        return Path.Combine(PublicZipDownloadsDirectoryPath(), "processing");
    }

    public string PublicZipDownloadsFailuresDirectoryPath() {
        return Path.Combine(PublicZipDownloadsDirectoryPath(), "failures");
    }

    public string PublicZipDownloadsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public", "zip-downloads");
    }
}
