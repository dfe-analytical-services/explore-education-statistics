namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string GetBasePath();
    
    // PublicTopLevelCalls
    string PublicApiTopLevelCallsDirectoryPath();

    string PublicApiTopLevelCallsReportsDirectoryPath();
    
    // PublicApiDataSetCalls
    string PublicApiDataSetCallsDirectoryPath();

    string PublicApiDataSetCallsReportsDirectoryPath();
    
    // PublicApiDataSetVersionCalls
    string PublicApiDataSetVersionCallsDirectoryPath();

    string PublicApiDataSetVersionCallsReportsDirectoryPath();
    
    // PublicApiQueries
    string PublicApiQueriesDirectoryPath();

    string PublicApiQueriesReportsDirectoryPath();

    // PublicZipDownloads
    string PublicZipDownloadsDirectoryPath();

    string PublicZipDownloadsReportsDirectoryPath();
}
