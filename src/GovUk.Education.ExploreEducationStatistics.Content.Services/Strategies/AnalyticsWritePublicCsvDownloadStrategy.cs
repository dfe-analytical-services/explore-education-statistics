#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public class AnalyticsWritePublicCsvDownloadStrategy( // @MarkFix write tests
    IAnalyticsPathResolver analyticsPathResolver,
    DateTimeProvider dateTimeProvider,
    ILogger<AnalyticsWritePublicCsvDownloadStrategy> logger
    ) : IAnalyticsWriteStrategy
{
    public Type RequestType => typeof(CaptureCsvDownloadRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (request is not CaptureCsvDownloadRequest csvDownloadRequest)
        {
            throw new ArgumentException($"request isn't a {nameof(CaptureCsvDownloadRequest)}");
        }

        logger.LogInformation(
            "Capturing {RequestTypeName} for releaseVersion {ReleaseVersionId} and subjectId {SubjectId}",
            csvDownloadRequest.GetType().ToString(),
            csvDownloadRequest.ReleaseVersionId,
            csvDownloadRequest.SubjectId);

        var directory = analyticsPathResolver.PublicCsvDownloadsDirectoryPath();
        var filename =
            $"{dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}_{csvDownloadRequest.ReleaseVersionId}_{csvDownloadRequest.SubjectId}_{RandomUtils.RandomString()}.json";

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            var serialisedRequest = JsonSerializationUtils.Serialize(
                obj: csvDownloadRequest,
                formatting: Formatting.Indented,
                orderedProperties: true,
                camelCase: true);

            await File.WriteAllTextAsync(
                filePath, contents: serialisedRequest, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error whilst writing {RequestTypeName} to disk",
                request!.GetType().ToString());
        }
    }
}
