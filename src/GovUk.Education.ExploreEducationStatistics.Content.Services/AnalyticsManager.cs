#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly ILogger<IAnalyticsManager> _logger;

    private IOptions<AnalyticsOptions> _analyticsOptions;

    public AnalyticsManager(
        ILogger<IAnalyticsManager> logger,
        IOptions<AnalyticsOptions> analyticsOptions)
    {
        _logger = logger;
        _analyticsOptions = analyticsOptions;
    }

    public void RecordReleaseVersionZipDownload(Guid releaseVersionId, IList<Guid>? fileIds = null)
    {
        var filesAsString = fileIds == null
            ? "ALL FILES!"
            : fileIds.JoinToString(' ');

        _logger.LogWarning(
            $"YES!!!! AnalyticsManager logging {this.GetType().FullName} for ReleaseVersion {releaseVersionId}\n" +
            $"with files {filesAsString}\n" +
            $"AND OPTIONS!!! Enabled: {_analyticsOptions.Value.Enabled}, BasePath: {_analyticsOptions.Value.BasePath}\n\n");
    }


}
