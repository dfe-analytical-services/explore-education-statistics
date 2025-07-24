#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

/// <summary>
/// To capture data relating to a zip download request for analytics.
/// </summary>
public record CaptureZipDownloadRequest : IAnalyticsCaptureRequest
{
    public string PublicationName = string.Empty;

    public Guid ReleaseVersionId;

    public string ReleaseName = string.Empty;

    public string? ReleaseLabel;

    public AnalyticsFromPage FromPage;

    public Guid? SubjectId = null;

    public string? DataSetTitle = null;
};

public record CaptureCsvDownloadRequest(
    string PublicationName,
    Guid ReleaseVersionId,
    string ReleaseName,
    string? ReleaseLabel,
    Guid SubjectId,
    string DataSetTitle) : IAnalyticsCaptureRequest;
