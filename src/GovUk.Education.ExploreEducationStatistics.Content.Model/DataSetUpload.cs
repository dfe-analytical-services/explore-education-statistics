#nullable enable
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

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

    public required long DataFileSizeInBytes { get; init; }

    public string DataFilePath =>
        $"{FileStoragePathUtils.FilesPath(ReleaseVersionId, FileType.Data)}{DataFileId}";

    public required Guid MetaFileId { get; init; }

    public required string MetaFileName { get; init; }

    public required long MetaFileSizeInBytes { get; init; }

    public string MetaFilePath =>
        $"{FileStoragePathUtils.FilesPath(ReleaseVersionId, FileType.Metadata)}{MetaFileId}";

    public Guid? ReplacingFileId { get; init; }

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<DataSetUploadStatus>))]
    public required DataSetUploadStatus Status { get; set; }

    public DataSetScreenerResponse? ScreenerResult { get; set; }

    public DateTime Created { get; set; }

    public required string UploadedBy { get; set; }
}

public enum DataSetUploadStatus
{
    SCREENING,
    FAILED_SCREENING,
    PENDING_REVIEW,
    PENDING_IMPORT,
}
