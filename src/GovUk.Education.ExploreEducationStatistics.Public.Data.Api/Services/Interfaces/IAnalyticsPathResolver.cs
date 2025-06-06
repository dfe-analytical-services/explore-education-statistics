namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string PublicApiTopLevelCallsDirectoryPath();
    
    string PublicApiDataSetCallsDirectoryPath();
    
    string PublicApiDataSetVersionCallsDirectoryPath();

    string PublicApiQueriesDirectoryPath();
}
