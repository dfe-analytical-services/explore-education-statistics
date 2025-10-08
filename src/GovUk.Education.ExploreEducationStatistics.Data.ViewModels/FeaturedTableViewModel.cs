namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public record FeaturedTableViewModel(
    Guid Id,
    string Name,
    string? Description,
    Guid SubjectId,
    Guid DataBlockId,
    Guid DataBlockParentId,
    int Order
);
