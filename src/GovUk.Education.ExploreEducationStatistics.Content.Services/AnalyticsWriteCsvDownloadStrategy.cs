#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsWriteCsvDownloadStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<AnalyticsWriteCsvDownloadStrategy> logger
    ) : IAnalyticsWriteStrategy

{
    public bool CanHandle(BaseCaptureRequest request)
    {
        return request is CaptureCsvDownloadRequest;
    }

    public async Task Report(BaseCaptureRequest request)
    {
        var csvDownloadRequest = request as CaptureCsvDownloadRequest
                                 ?? throw new ArgumentException($"request isn't a {nameof(CaptureCsvDownloadRequest)}");

        logger.LogInformation(
            "Capturing {RequestTypeName} for releaseVersion {ReleaseVersionId} for subject {SubjectId}",
            csvDownloadRequest.GetType().ToString(),
            csvDownloadRequest.ReleaseVersionId,
            csvDownloadRequest.SubjectId);

        var serialisedRequest = JsonSerializationUtils.Serialize(
            obj: csvDownloadRequest,
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true);

        var directory = analyticsPathResolver.PublicCsvDownloadsDirectoryPath();

        var filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}_{csvDownloadRequest.ReleaseVersionId}_{csvDownloadRequest.SubjectId}.json";

        await AnalyticsUtils.WriteToFileShare(
            csvDownloadRequest.GetType().ToString(),
            directory,
            filename,
            serialisedRequest,
            logger);
    }
}
