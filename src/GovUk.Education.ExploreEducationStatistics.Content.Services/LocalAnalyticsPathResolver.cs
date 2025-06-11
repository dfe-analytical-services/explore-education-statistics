#nullable enable
using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class LocalAnalyticsPathResolver : AnalyticsPathResolverBase
{
    private readonly string _basePath;

    public LocalAnalyticsPathResolver(IOptions<AnalyticsOptions> options)
    {

        if (options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'Config for {nameof(AnalyticsOptions.BasePath)}' from {nameof(AnalyticsOptions)} must not be blank",
                paramName: nameof(options)
            );
        }

        var originalPath = options.Value.BasePath;
        _basePath = Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(originalPath));
    }

    protected override string GetBasePath()
    {
        return _basePath;
    }
}
