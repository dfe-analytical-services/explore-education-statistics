namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string GetBasePath();
    
    // PublicApiTopLevelCalls
    string PublicApiTopLevelCallsDirectoryPath();

    string PublicApiTopLevelCallsReportsDirectoryPath();

    // PublicApiPublicationLevelCalls
    string PublicApiPublicationCallsDirectoryPath();
    
    string PublicApiPublicationCallsReportsDirectoryPath();
    
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

    // PublicCsvDownloads
    string PublicCsvDownloadsDirectoryPath();

    string PublicCsvDownloadsReportsDirectoryPath();
}
