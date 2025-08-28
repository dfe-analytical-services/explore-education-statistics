#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record DataSetCandidateViewModel
{
    public required Guid ReleaseFileId { get; init; }

    public required string Title { get; init; }
}
