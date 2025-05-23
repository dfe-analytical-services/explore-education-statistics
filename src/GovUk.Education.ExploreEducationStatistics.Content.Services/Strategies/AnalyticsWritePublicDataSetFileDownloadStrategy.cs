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

public class AnalyticsWritePublicDataSetFileDownloadStrategy( // @MarkFix write tests
    IAnalyticsPathResolver analyticsPathResolver,
    DateTimeProvider dateTimeProvider,
    ILogger<AnalyticsWritePublicDataSetFileDownloadStrategy> logger
    ) : IAnalyticsWriteStrategy
{
    public Type RequestType => typeof(CaptureDataSetFileDownloadRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (request is not CaptureDataSetFileDownloadRequest dataSetFileDownloadRequest)
        {
            throw new ArgumentException($"request isn't a {nameof(CaptureDataSetFileDownloadRequest)}");
        }

        logger.LogInformation(
            "Capturing {RequestTypeName} for releaseVersion {ReleaseVersionId} and subjectId {SubjectId}",
            dataSetFileDownloadRequest.GetType().ToString(),
            dataSetFileDownloadRequest.ReleaseVersionId,
            dataSetFileDownloadRequest.SubjectId);

        var directory = analyticsPathResolver.PublicDataSetFileDownloadsDirectoryPath();
        var filename =
            $"{dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}_{dataSetFileDownloadRequest.ReleaseVersionId}_{dataSetFileDownloadRequest.SubjectId}_{RandomUtils.RandomString()}.json";

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            var serialisedRequest = JsonSerializationUtils.Serialize(
                obj: dataSetFileDownloadRequest,
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
