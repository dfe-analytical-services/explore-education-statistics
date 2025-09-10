#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetUploadViewModel
{
    public Guid Id { get; init; }

    public required string DataSetTitle { get; init; }

    public required string DataFileName { get; init; }

    public required string DataFileSize { get; init; }

    public required string MetaFileName { get; init; }

    public required string MetaFileSize { get; init; }

    public required string Status { get; set; }

    public ScreenerResultViewModel? ScreenerResult { get; set; }

    public required DateTime Created { get; set; }

    public required string UploadedBy { get; set; }

    public Guid? ReplacingFileId { get; init; }
}
