using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
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

    private string BasePath() => _basePath;

    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(BasePath(), "public", "zip-downloads");
    }

    public string PublicCsvDownloadsDirectoryPath()
    {
        return Path.Combine(BasePath(), "public", "csv-downloads");
    }
}
