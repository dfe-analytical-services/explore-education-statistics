using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseFileViewModel
{
    public required Guid Id { get; init; }

    public required ReleaseSummaryViewModel Release { get; init; }

    public required FileInfo File { get; init; }

    public Guid? DataSetFileId { get; init; }
}
