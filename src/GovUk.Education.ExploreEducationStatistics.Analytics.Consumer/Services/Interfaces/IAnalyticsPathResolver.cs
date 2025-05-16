namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BasePath();

    // PublicApiQueries
    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();
    
    // PublicApiDataSetVersionCalls
    string PublicApiDataSetVersionCallsDirectoryPath();

    string PublicApiDataSetVersionCallsReportsDirectoryPath();

    // PublicZipDownloads
    string PublicZipDownloadsDirectoryPath();

    string PublicZipDownloadsReportsDirectoryPath();
}
