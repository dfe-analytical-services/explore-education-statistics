using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Options;
using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services;

public class AnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly IOptions<AnalyticsOptions> _options;
    private readonly IHostEnvironment _environment;
    private readonly string _basePath;

    public AnalyticsPathResolver(IOptions<AnalyticsOptions> options, IHostEnvironment environment)
    {
        _options = options;
        _environment = environment;

        if (_options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'{nameof(AnalyticsOptions.BasePath)}' must not be blank",
                paramName: nameof(options)
            );
        }

        _basePath = GetBasePath();
    }

    private string BasePath() => _basePath;

    public string PublicApiQueriesDirectoryPath()
    {
        return Path.Combine(BasePath(), "public-api", "queries");
    }

    public string PublicApiQueriesProcessingDirectoryPath() {
        return Path.Combine(BasePath(), "public-api", "queries", "processing");
    }

    public string PublicApiQueriesReportsDirectoryPath() {
        return Path.Combine(BasePath(), "reports", "public-api", "queries");
    }

    private string GetBasePath()
    {
        if (_environment.IsDevelopment())
        {
            return Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(_options.Value.BasePath));
        }

        if (_environment.IsIntegrationTest())
        {
            var randomTestInstanceDir = Guid.NewGuid().ToString();
            return Path.Combine(
                Path.GetTempPath(),
                "ExploreEducationStatistics",
                PathUtils.OsPath(_options.Value.BasePath),
                randomTestInstanceDir
            );
        }

        return _options.Value.BasePath;
    }
}
