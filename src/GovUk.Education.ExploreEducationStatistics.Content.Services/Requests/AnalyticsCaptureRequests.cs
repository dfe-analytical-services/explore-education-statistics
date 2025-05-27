#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

/// <summary>
/// To capture data relating to a zip download request for analytics.
/// </summary>
public record CaptureZipDownloadRequest : IAnalyticsCaptureRequestBase
{
    public string PublicationName = string.Empty;

    public Guid ReleaseVersionId;

    public string ReleaseName = string.Empty;

    public string? ReleaseLabel;

    [JsonConverter(typeof(StringEnumConverter))]
    public AnalyticsFromPage FromPage;

    public Guid? SubjectId = null;

    public string? DataSetTitle = null;
};

public record CaptureDataSetFileDownloadRequest(
    string PublicationName,
    Guid ReleaseVersionId,
    string ReleaseName,
    string? ReleaseLabel,
    Guid SubjectId,
    string DataSetTitle) : IAnalyticsCaptureRequestBase;
