#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using System;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

/// <summary>
/// Represents a data set which has been uploaded to temporary storage, pending validation and screening.
/// </summary>
public record DataSetUpload : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; init; }

    public required Guid ReleaseVersionId { get; init; }

    public required string DataSetTitle { get; init; }

    public required Guid DataFileId { get; init; }

    public required string DataFileName { get; init; }

    public string DataFilePath
        => $"{FileStoragePathUtils.FilesPath(ReleaseVersionId, FileType.Data)}{DataFileId}";

    public required Guid MetaFileId { get; init; }

    public required string MetaFileName { get; init; }

    public string MetaFilePath
        => $"{FileStoragePathUtils.FilesPath(ReleaseVersionId, FileType.Metadata)}{MetaFileId}";

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<DataSetUploadStatus>))]
    public required DataSetUploadStatus Status { get; set; }

    public DateTime Created { get; set; }
}

public enum DataSetUploadStatus
{
    Screening,
    CompletePendingReview,
    CompletePendingImport,
    Failed,
}
