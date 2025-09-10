#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record FeaturedTableViewModel(
    Guid Id,
    string Name,
    string? Description,
    int Order,
    Guid DataBlockId,
    Guid DataBlockParentId);

public record FeaturedTableBasicViewModel(string Name, string? Description);