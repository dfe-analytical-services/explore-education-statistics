namespace GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string PublicApiQueriesDirectoryPath();
    
    string PublicApiQueriesProcessingDirectoryPath();
    
    string PublicApiQueriesReportsDirectoryPath();
}
