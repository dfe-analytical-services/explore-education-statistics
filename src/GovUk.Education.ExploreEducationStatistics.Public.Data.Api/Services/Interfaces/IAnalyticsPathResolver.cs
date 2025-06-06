namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string PublicApiTopLevelCallsDirectoryPath();
    
    string PublicApiPublicationCallsDirectoryPath();
    
    string PublicApiDataSetCallsDirectoryPath();
    
    string PublicApiDataSetVersionCallsDirectoryPath();

    string PublicApiQueriesDirectoryPath();
}
