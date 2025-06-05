using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public abstract class AnalyticsPathResolverBase : IAnalyticsPathResolver
{
    protected abstract string GetBasePath();

    public string PublicApiTopLevelCallsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public-api", "top-level");
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
