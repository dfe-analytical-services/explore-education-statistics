using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Options;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class AnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath;

    public AnalyticsPathResolver(IOptions<AnalyticsOptions> options, IHostEnvironment environment)
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

    public string BasePath()
    {
        return _basePath;
    }

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "queries");
    }

    public string PublicApiQueriesProcessingDirectoryPath()
    {
        return Path.Combine(PublicApiQueriesDirectoryPath(), "processing");
    }

    public string PublicApiQueriesFailuresDirectoryPath()
    {
        return Path.Combine(PublicApiQueriesDirectoryPath(), "failures");
    }

    public string PublicApiQueriesReportsDirectoryPath()
    {
        return Path.Combine(ReportsDirectoryPath(), "public-api", "queries");
    }
    
    private string ReportsDirectoryPath()
    {
        return Path.Combine(_basePath, "reports");
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
