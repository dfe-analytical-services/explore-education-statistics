namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BasePath();

    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesProcessingDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();

    string PublicZipDownloadsDirectoryPath();

    string PublicZipDownloadsProcessingDirectoryPath();

    string PublicZipDownloadsReportsDirectoryPath();

    string PublicCsvDownloadsDirectoryPath();

    string PublicCsvDownloadsProcessingDirectoryPath();

    string PublicCsvDownloadsReportsDirectoryPath();
}
