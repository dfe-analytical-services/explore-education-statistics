using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileSummaryViewModel
{
    public required Guid Id { get; init; }

    public required Guid FileId { get; init; }

    public required string Filename { get; init; }

    public required string FileSize { get; init; }

    public string FileExtension => Path.GetExtension(Filename).TrimStart('.');

    public required string Title { get; init; }

    public required string Content { get; init; }

    public required IdTitleViewModel Theme { get; init; }

    public required IdTitleSlugViewModel Publication { get; init; }

    public required IdTitleSlugViewModel Release { get; init; }

    public required bool LatestData { get; init; }

    public required bool IsSuperseded { get; init; }

    public required DateTime Published { get; init; }

    public DateTime LastUpdated { get; init; }

    public required DataSetFileMetaViewModel Meta { get; init; }

    public DataSetFileApiViewModel? Api { get; init; }
}
