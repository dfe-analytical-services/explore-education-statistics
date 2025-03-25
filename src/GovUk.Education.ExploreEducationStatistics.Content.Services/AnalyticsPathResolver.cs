using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

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

    public string ZipDownloadsDirectoryPath()
    {
        return Path.Combine(BasePath(), "public", "zip-downloads");
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
