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

    public string PublicApiQueriesReportsDirectoryPath()
    {
        return Path.Combine(ReportsDirectoryPath(), "public-api", "queries");
    }
    
    // PublicApiDataSets
    public string PublicApiDataSetCallsDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "data-sets");
    }

    public string PublicApiDataSetCallsReportsDirectoryPath()
    {
        return Path.Combine(PublicApiDataSetCallsDirectoryPath(), "public-api", "data-sets");
    }

    // PublicApiDataSetVersions
    public string PublicApiDataSetVersionCallsDirectoryPath()
    {
        return Path.Combine(_basePath, "public-api", "data-set-versions");
    }

    public string PublicApiDataSetVersionCallsReportsDirectoryPath()
    {
        return Path.Combine(PublicApiDataSetVersionCallsDirectoryPath(), "public-api", "data-set-versions");
    }

    // PublicZipDownloads
    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "zip-downloads");
    }

    public string PublicZipDownloadsReportsDirectoryPath()
    {
        return Path.Combine(ReportsDirectoryPath(), "public", "zip-downloads");
    }

    // PublicDataSetFileDownloads
    public string PublicCsvDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "public", "csv-downloads");
    }

    public string PublicCsvDownloadsReportsDirectoryPath()
    {
        return Path.Combine(ReportsDirectoryPath(), "public", "csv-downloads");
    }
}
