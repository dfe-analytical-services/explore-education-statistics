namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BasePath();

    // PublicApiQueries
    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesProcessingDirectoryPath();

    string PublicApiQueriesFailuresDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();
    
    // PublicApiDataSetVersionCalls
    string PublicApiDataSetVersionCallsDirectoryPath();

    string PublicApiDataSetVersionCallsProcessingDirectoryPath();

    string PublicApiDataSetVersionCallsFailuresDirectoryPath();

    string PublicApiDataSetVersionCallsReportsDirectoryPath();

    // PublicZipDownloads
    string PublicZipDownloadsDirectoryPath();

    string PublicZipDownloadsProcessingDirectoryPath();

    string PublicZipDownloadsFailuresDirectoryPath();

    string PublicZipDownloadsReportsDirectoryPath();
}
