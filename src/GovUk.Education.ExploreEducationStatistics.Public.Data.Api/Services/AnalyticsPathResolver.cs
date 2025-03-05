using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath;

    public AnalyticsPathResolver(IOptions<AnalyticsOptions> options, IWebHostEnvironment environment)
    {
        if (options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'{nameof(AnalyticsOptions.BasePath)}' must not be blank",
                paramName: nameof(options)
            );
        }
        
        _basePath = GetBasePath(options.Value.BasePath, environment);
    }

    private string BasePath() => _basePath;

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(BasePath(), "public-api", "queries");
    }
    
    private string GetBasePath(string originalPath, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return originalPath;
        }
        
        return Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(originalPath));
    }
}
