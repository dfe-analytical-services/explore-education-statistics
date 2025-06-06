using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public abstract class AnalyticsPathResolverBase : IAnalyticsPathResolver
{
    public abstract string GetBasePath();

    private static readonly string[] PublicApiTopLevelSubPath = ["public-api", "top-level"];
    private static readonly string[] PublicApiDataSetsSubPath = ["public-api", "data-sets"];
    private static readonly string[] PublicApiDataSetVersionsSubPath = ["public-api", "data-set-versions"];
    private static readonly string[] PublicApiQueriesSubPath = ["public-api", "queries"];
    private static readonly string[] PublicZipDownloadsSubPath = ["public", "zip-downloads"];
    private static readonly string[] PublicCsvDownloadsSubPath = ["public", "csv-downloads"];
        
    // PublicApiTopLevel
    public string PublicApiTopLevelCallsDirectoryPath()
    {
        return Path.Combine([GetBasePath(), ..PublicApiTopLevelSubPath]);
    }

    public string PublicApiTopLevelCallsReportsDirectoryPath()
    {
        return Path.Combine([ReportsDirectoryPath(), ..PublicApiTopLevelSubPath]);
    }
    
    // PublicApiDataSets
    public string PublicApiDataSetCallsDirectoryPath()
    {
        return Path.Combine([GetBasePath(), ..PublicApiDataSetsSubPath]);
    }

    public string PublicApiDataSetCallsReportsDirectoryPath()
    {
        return Path.Combine([ReportsDirectoryPath(), ..PublicApiDataSetsSubPath]);
    }

    // PublicApiDataSetVersions
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine([GetBasePath(), ..PublicApiDataSetVersionsSubPath]);
    }

    public string PublicApiDataSetVersionCallsReportsDirectoryPath()
    {
        return Path.Combine([ReportsDirectoryPath(), ..PublicApiDataSetVersionsSubPath]);
    }

    // PublicApiQueries
    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine([GetBasePath(), ..PublicApiQueriesSubPath]);
    }

    public string PublicApiQueriesReportsDirectoryPath()
    {
        return Path.Combine([ReportsDirectoryPath(), ..PublicApiQueriesSubPath]);
    }

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine([GetBasePath(), ..PublicZipDownloadsSubPath]);
    }

    public string PublicZipDownloadsReportsDirectoryPath()
    {
        return Path.Combine([ReportsDirectoryPath(), ..PublicZipDownloadsSubPath]);
    }

    // PublicCsvDownloads
    public string PublicCsvDownloadsDirectoryPath()
    {
        return Path.Combine([GetBasePath(), ..PublicCsvDownloadsSubPath]);
    }

    public string PublicCsvDownloadsReportsDirectoryPath()
    {
        return Path.Combine([ReportsDirectoryPath(), ..PublicCsvDownloadsSubPath]);
    }
    
    private string ReportsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "reports");
    }
}
