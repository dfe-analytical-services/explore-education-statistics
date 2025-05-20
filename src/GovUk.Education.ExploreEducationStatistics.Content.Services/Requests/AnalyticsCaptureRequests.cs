#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

public interface IAnalyticsCaptureRequestBase;

/// <summary>
/// To capture data relating to a zip download request for analytics.
/// </summary>
public record CaptureZipDownloadRequest(
    string PublicationName,
    Guid ReleaseVersionId,
    string ReleaseName,
    string? ReleaseLabel,
    Guid? SubjectId = null,
    string? DataSetTitle = null) : IAnalyticsCaptureRequestBase;
