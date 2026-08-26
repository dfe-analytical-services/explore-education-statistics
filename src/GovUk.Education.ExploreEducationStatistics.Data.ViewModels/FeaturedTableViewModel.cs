namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public record FeaturedTableViewModel(
    Guid Id,
    string Name,
    string? Description,
    Guid SubjectId,
    Guid DataBlockVersionId,
    Guid DataBlockId,
    int Order
);
