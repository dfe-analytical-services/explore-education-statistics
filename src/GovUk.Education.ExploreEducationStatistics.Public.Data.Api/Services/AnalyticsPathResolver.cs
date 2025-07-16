using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

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
    public string BuildOutputDirectory(string[] subPaths) => Path.Combine([_basePath, ..subPaths]);
}
