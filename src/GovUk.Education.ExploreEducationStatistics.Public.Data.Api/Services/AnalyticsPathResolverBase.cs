using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public abstract class AnalyticsPathResolverBase : IAnalyticsPathResolver
{
    protected abstract string GetBasePath();

    public string PublicApiTopLevelCallsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public-api", "top-level");
    }

    public string PublicApiPublicationCallsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public-api", "publications");
    }

    public string PublicApiDataSetCallsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public-api", "data-sets");
    }
    
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public-api", "data-set-versions");
    }

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public-api", "queries");
    }
}
