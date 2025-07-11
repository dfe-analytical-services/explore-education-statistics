using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class AnalyticsPathResolver : AnalyticsPathResolverBase
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

    public override string GetBasePath()
    {
        return _basePath;
    }
}
