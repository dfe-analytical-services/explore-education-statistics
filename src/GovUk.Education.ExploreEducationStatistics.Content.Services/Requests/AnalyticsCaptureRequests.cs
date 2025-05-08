#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

/// <summary>
/// To capture data relating to a zip download request for analytics.
/// </summary>
public record CaptureZipDownloadRequest(
    string PublicationName,
    Guid ReleaseVersionId,
    string ReleaseName,
    string? ReleaseLabel,
    Guid? SubjectId = null,
    string? DataSetTitle = null,
    // TODO: FromPage can be made non-optional later
    string? FromPage = null) : IAnalyticsCaptureRequestBase;
