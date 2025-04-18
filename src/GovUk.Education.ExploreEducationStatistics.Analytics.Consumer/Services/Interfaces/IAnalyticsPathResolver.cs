namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BasePath();

    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesProcessingDirectoryPath();

    string PublicApiQueriesFailuresDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();
}
