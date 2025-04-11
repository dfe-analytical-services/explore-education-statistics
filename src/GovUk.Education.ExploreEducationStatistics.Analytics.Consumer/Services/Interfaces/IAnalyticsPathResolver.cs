namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BasePath();

    // PublicApiQueries
    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesProcessingDirectoryPath();

    string PublicApiQueriesFailuresDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();

    // PublicZipDownloads
    string PublicZipDownloadsDirectoryPath();

    string PublicZipDownloadsProcessingDirectoryPath();

    string PublicZipDownloadsFailuresDirectoryPath();

    string PublicZipDownloadsReportsDirectoryPath();
}
