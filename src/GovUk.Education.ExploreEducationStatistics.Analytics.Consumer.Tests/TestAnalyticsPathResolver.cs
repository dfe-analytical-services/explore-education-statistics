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
    
    // PublicApiDataSets
    public string PublicApiDataSetCallsDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "data-sets");
    }

    public string PublicApiDataSetCallsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "data-sets");
    }
    
    // PublicApiDataSetVersions
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "data-set-versions");
    }

    public string PublicApiDataSetVersionCallsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public-api", "data-set-versions");
    }

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsReportsDirectoryPath() {
        return Path.Combine(_basePath, "reports", "public", "zip-downloads");
    }

    // PublicDataSetFileDownloads
    public string PublicDataSetFileDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "csv-downloads");
    }

    public string PublicDataSetFileDownloadsReportsDirectoryPath()
    {
        return Path.Combine(_basePath, "reports", "public", "csv-downloads");
    }
}
