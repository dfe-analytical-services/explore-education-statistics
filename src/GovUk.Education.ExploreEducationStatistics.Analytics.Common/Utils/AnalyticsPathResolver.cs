using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;

public class AnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath;

    public AnalyticsPathResolver(IOptions<AnalyticsOptions> options)
    {
        if (options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'Config for {nameof(AnalyticsOptions.BasePath)}' from {nameof(AnalyticsOptions)} must not be blank",
                paramName: nameof(options)
            );
        }

        _basePath = options.Value.BasePath;
    }

    public string BuildOutputDirectory(string[] subPaths) => Path.Combine([_basePath, .. subPaths]);
}
