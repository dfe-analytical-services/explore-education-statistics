using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class LocalAnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath;

    public LocalAnalyticsPathResolver(IOptions<AnalyticsOptions> options)
    {
        if (options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'{nameof(AnalyticsOptions.BasePath)}' must not be blank",
                paramName: nameof(options)
            );
        }
        
        var originalPath = options.Value.BasePath;
        _basePath = Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(originalPath));
    }

    private string BasePath() => _basePath;

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(BasePath(), "public-api", "queries");
    }
    
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine(BasePath(), "public-api", "data-set-versions");
    }
}
