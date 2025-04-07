#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsWriter(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<IAnalyticsWriter> logger
    ) : IAnalyticsWriter
{

    public async Task ReportReleaseVersionZipDownload(CaptureReleaseVersionZipDownloadRequest request)
    {
        logger.LogInformation(
            "Capturing ReleaseVersionZipDownload event analytics for releaseVersion {ReleaseVersionId}{WithSubject}",
            request.ReleaseVersionId,
            request.SubjectId == null
                ? ""
                : " for subject " + request.SubjectId);

        var serialisedRequest = JsonSerializationUtils.Serialize(
            obj: request,
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true);

        var directory = analyticsPathResolver.PublicZipDownloadsDirectoryPath();

        var filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}_{request.ReleaseVersionId}.json";

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);
            if (File.Exists(filePath))
            {
                throw new Exception($"Tried to create a file that already exists for {nameof(CaptureReleaseVersionZipDownloadRequest)}!");
            }

            await File.WriteAllTextAsync(
                filePath,
                contents: serialisedRequest);
        }
        catch (Exception e)
        {
            logger.LogError(
                exception: e,
                message: "Error whilst writing {ReleaseVersionZipDownloadRequest} to disk",
                nameof(CaptureReleaseVersionZipDownloadRequest));
        }
    }

    public record CaptureReleaseVersionZipDownloadRequest(
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string? ReleaseLabel,
        Guid? SubjectId = null,
        string? DataSetName = null);
}
