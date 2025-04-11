#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public class AnalyticsWritePublicZipDownloadStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<AnalyticsWritePublicZipDownloadStrategy> logger
    ) : IAnalyticsWriteStrategy
{
    public bool CanHandle(BaseCaptureRequest request)
    {
        return request is CaptureZipDownloadRequest;
    }

    public async Task Report(BaseCaptureRequest request, CancellationToken cancellationToken)
    {
        var zipDownloadRequest = request as CaptureZipDownloadRequest
                                 ?? throw new ArgumentException($"request isn't a {nameof(CaptureZipDownloadRequest)}");

        logger.LogInformation(
            "Capturing {RequestTypeName} for releaseVersion {ReleaseVersionId}{WithSubject}",
            zipDownloadRequest.GetType().ToString(),
            zipDownloadRequest.ReleaseVersionId,
            zipDownloadRequest.SubjectId == null
                ? ""
                : " for subject " + zipDownloadRequest.SubjectId);

        var serialisedRequest = JsonSerializationUtils.Serialize(
            obj: zipDownloadRequest,
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true);

        var directory = analyticsPathResolver.PublicZipDownloadsDirectoryPath();

        var filename =
            $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{zipDownloadRequest.ReleaseVersionId}_{RandomUtils.RandomString()}.json";

        await AnalyticsUtils.WriteToFileShare(
            request.GetType().ToString(),
            directory,
            filename,
            serialisedRequest,
            logger);
    }
}
