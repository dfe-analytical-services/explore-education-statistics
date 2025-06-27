#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsPathResolver : AnalyticsPathResolverBase
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

    protected override string GetBasePath()
    {
        return _basePath;
    }
}
