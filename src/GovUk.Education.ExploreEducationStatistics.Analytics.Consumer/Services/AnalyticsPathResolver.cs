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
        
        var originalPath = options.Value.BasePath;
        if (environment.IsDevelopment())
        {
            _basePath = Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(originalPath));
        }
        else
        {
            _basePath = originalPath;
        }

    }

    public string BasePath()
    {
        return _basePath;
    }

    private string ReportsDirectoryPath()
    {
        return Path.Combine(_basePath, "reports");
    }

    // PublicApiQueries
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

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsProcessingDirectoryPath()
    {
        return Path.Combine(PublicZipDownloadsDirectoryPath(), "processing");
    }

    public string PublicZipDownloadsFailuresDirectoryPath()
    {
        return Path.Combine(PublicZipDownloadsDirectoryPath(), "failures");
    }

    public string PublicZipDownloadsReportsDirectoryPath()
    {
        return Path.Combine(ReportsDirectoryPath(), "public", "zip-downloads");
    }
}
