#nullable enable
using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Config;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Utils;

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

    public string GetPermaLinkTableDownloadCallsDirectoryPath() =>
        Path.Combine(_basePath, "public", "table-tool-downloads", "permalinks");

    public string GetTableToolDownloadCallsDirectoryPath() => 
        Path.Combine(_basePath, "public", "table-tool-downloads", "table-tool-page");
}
