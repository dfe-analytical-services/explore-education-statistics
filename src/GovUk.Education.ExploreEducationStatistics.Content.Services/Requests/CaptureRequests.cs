#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

public abstract record BaseCaptureRequest;

public record CaptureZipDownloadRequest(
    string PublicationName,
    Guid ReleaseVersionId,
    string ReleaseName,
    string? ReleaseLabel,
    Guid? SubjectId = null,
    string? DataSetName = null) : BaseCaptureRequest;
