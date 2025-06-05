namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BasePath();

    // PublicApiQueries
    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();
    
    // PublicApiDataSetCalls
    string PublicApiDataSetCallsDirectoryPath();

    string PublicApiDataSetCallsReportsDirectoryPath();
    
    // PublicApiDataSetVersionCalls
    string PublicApiDataSetVersionCallsDirectoryPath();

    string PublicApiDataSetVersionCallsReportsDirectoryPath();

    // PublicZipDownloads
    string PublicZipDownloadsDirectoryPath();

    string PublicZipDownloadsReportsDirectoryPath();

    // PublicCsvDownloads
    string PublicCsvDownloadsDirectoryPath();

    string PublicCsvDownloadsReportsDirectoryPath();
}
