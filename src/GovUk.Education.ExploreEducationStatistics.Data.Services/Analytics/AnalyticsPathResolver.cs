#nullable enable
using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;

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

    public string GetPermaLinkTableDownloadCallsDirectoryPath()
    {
        throw new NotImplementedException();
        return Path.Combine(_basePath, "public", "csv-downloads");
    }

    public string GetTableToolDownloadCallsDirectoryPath() => throw new NotImplementedException();
}
