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

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "queries");
    }
    
    // PublicApiQueries
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "data-set-version-calls");
    }

    public string PublicApiDataSetVersionCallsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "data-set-version-calls");
    }

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public", "zip-downloads");
    }
}
